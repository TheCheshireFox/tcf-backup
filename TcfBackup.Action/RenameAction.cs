using System.Globalization;
using System.Text;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action;

public class RenameAction : IAction
{
    private static readonly Dictionary<string, Func<string, string?, string>> s_templateReplacers = new()
    {
        { "filename", FormatFilename },
        { "filename_without_ext", FormatFilenameWithoutExt },
        { "ext", FormatExtension },
        { "date", FormatDate }
    };

    private readonly ILogger _logger;
    private readonly IFilesystem _fs;
    private readonly string _template;
    private readonly bool _overwrite;

    private static string ApplyFormat<T>(T input, string? format) => string.IsNullOrEmpty(format) ? input?.ToString() ?? "" : string.Format($"{{0:{format}}}", input);
    private static string FormatFilename(string filename, string? format) => ApplyFormat(filename, format);
    private static string FormatFilenameWithoutExt(string filename, string? format) => ApplyFormat(Path.GetFileNameWithoutExtension(filename), format);
    private static string FormatExtension(string filename, string? format) => ApplyFormat(PathUtils.GetFullExtension(filename)[1..], format);

    private static string FormatDate(string filename, string? format) => string.IsNullOrEmpty(format)
        ? DateTime.Now.ToString("s", CultureInfo.InvariantCulture)
        : DateTime.Now.ToString(format, CultureInfo.InvariantCulture);

    private static (string? Placeholder, string? Format) SplitPlaceholder(string placeholder)
    {
        var index = placeholder.IndexOf(':');

        switch (index)
        {
            case -1:
                return (placeholder, null);
            case 0:
                return (null, placeholder);
        }

        while (index > 0 && placeholder[index - 1] == '\\')
        {
            index = placeholder.IndexOf(':', index + 1);
        }

        return index == -1
            ? (placeholder, null)
            : (placeholder[..index], placeholder[(index + 1)..]);
    }

    private static string Format(string template, string filename)
    {
        var replacements = new List<(int Start, int Length, string Value)>();

        var index = -1;
        var closeIndex = -1;
        while ((index = template.IndexOf('{', index + 1)) >= 0)
        {
            if (index > 0 && template[index - 1] == '\\')
            {
                continue;
            }

            closeIndex = index;
            while (true)
            {
                closeIndex = template.IndexOf('}', closeIndex);
                if (closeIndex == -1)
                {
                    break;
                }

                if (closeIndex > 0 && template[closeIndex - 1] == '\\')
                {
                    continue;
                }

                break;
            }

            if (closeIndex == -1)
            {
                throw new FormatException($"Unclosed opening bracket at pos {index}");
            }

            var param = template[(index + 1)..closeIndex];
            var (placeholder, format) = SplitPlaceholder(param);

            if (placeholder == null)
            {
                throw new FormatException($"No placeholder name in {placeholder}");
            }

            replacements.Add((index, closeIndex - index, s_templateReplacers[placeholder](filename, format)));
        }

        var sb = new StringBuilder();
        var lastIndex = 0;
        foreach (var (start, length, value) in replacements.OrderBy(r => r.Start))
        {
            sb.Append(template.AsSpan(lastIndex, start - lastIndex));
            sb.Append(value);
            lastIndex = start + length + 1;
        }

        if (closeIndex != template.Length)
        {
            sb.Append(template[(closeIndex + 1)..]);
        }

        var result = sb.ToString();
        if (result.Contains(Path.DirectorySeparatorChar))
        {
            throw new FormatException($"Formatting results in invalid filename: {result}");
        }

        return result;
    }

    public RenameAction(ILogger logger, IFilesystem fs, string template, bool overwrite)
    {
        _logger = logger.ForContextShort<RenameAction>();
        _fs = fs;
        _template = template;
        _overwrite = overwrite;
    }

    public ISource Apply(ISource source)
    {
        _logger.Information("Renaming files...");

        var renames = source.GetFiles().ToDictionary(f => f, f => Path.Combine(Path.GetDirectoryName(f.Path) ?? "/", Format(_template, Path.GetFileName(f.Path))));
        foreach (var (src, dst) in renames)
        {
            if (src.Path.Equals(dst))
            {
                continue;
            }

            src.Move(dst, _overwrite);
        }

        _logger.Information("Renaming complete");
        return FilesListSource.CreateMutable(_fs, renames.Values);
    }
}