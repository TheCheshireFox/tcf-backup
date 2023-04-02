using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action;

internal static unsafe class LibC
{
    private const int TmSize = 56;
    
    [DllImport("libc", EntryPoint = "localtime_r", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern byte* localtime_r(ref long unixTime, byte* tm);
    
    [DllImport("libc", EntryPoint = "gmtime_r", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern byte* gmtime_r(ref long unixTime, byte* tm);
    
    [DllImport("libc", EntryPoint = "strftime", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int strftime(byte* buffer, int size, byte* format, byte* tm);
    
    public static string FormatDate(DateTime date, string? format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return string.Empty;
        }
        
        var tm = stackalloc byte[TmSize];
        var unixTime = new DateTimeOffset(date).ToUnixTimeSeconds();
        tm = date.Kind == DateTimeKind.Utc
            ? gmtime_r(ref unixTime, tm)
            : localtime_r(ref unixTime, tm);

        if (tm == null)
        {
            throw new FormatException();
        }
        
        fixed (byte* pFormat = Encoding.UTF8.GetBytes(format))
        {
            var size = format.Length;
            while (true)
            {
                var neededSize = strftime(null, unchecked(size *= 2), pFormat, tm);
                if (neededSize == 0)
                {
                    if (size < 0)
                    {
                        throw new FormatException();
                    }
                    continue;
                }
                
                size = neededSize;
                break;
            }

            fixed (byte* pBuffer = new byte[++size])
            {
                if (strftime(pBuffer, size, pFormat, tm) == 0)
                {
                    throw new FormatException();
                }

                return Encoding.UTF8.GetString(pBuffer, size - 1);
            }
        }
    }
}

public class RenameAction : IAction
{
    private static readonly Dictionary<string, Func<string, string?, string>> s_templatePlaceholders = new()
    {
        { "name", (name, _) => name },
        { "stem", FormatFilenameWithoutExt },
        { "ext", FormatExtension },
        { "date", FormatDate }
    };

    private readonly ILogger _logger;
    private readonly IFileSystem _fs;
    private readonly string _template;
    private readonly bool _overwrite;
    
    private static string FormatFilenameWithoutExt(string filename, string? format) => PathUtils.GetFileNameWithoutExtension(filename) ?? string.Empty;
    private static string FormatExtension(string filename, string? format) => PathUtils.GetFullExtension(filename)[1..];

    private static string FormatDate(string filename, string? format) => LibC.FormatDate(DateTime.Now, format);

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
                throw new FormatException($"No placeholder name in {param}");
            }

            replacements.Add((index, closeIndex - index, s_templatePlaceholders[placeholder](filename, format)));
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

    private async Task ApplyAsync(IFileListSource source, IActionContext actionContext, CancellationToken cancellationToken)
    {
        _logger.Information("Renaming files...");
        
        var targetDir = _fs.GetTempPath();
        try
        {
            var renames = new ConcurrentDictionary<IFile, string>();
            await Parallel.ForEachAsync(source.GetFiles(), cancellationToken, (file, _) =>
            {
                renames.TryAdd(file, Format(_template, Path.GetFileName(file.Path)));
                return ValueTask.CompletedTask;
            });

            foreach (var (src, dst) in renames)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (src.Path.Equals(dst))
                {
                    continue;
                }

                _logger.Information("{Src} -> {Dst}...", src.Path, dst);
                src.Move(dst, _overwrite);
            }

            actionContext.SetResult(FilesListSource.CreateMutable(_fs, targetDir));
            
            _logger.Information("Complete");
        }
        catch (Exception)
        {
            try
            {
                _fs.Directory.Delete(targetDir);
            }
            catch (Exception)
            {
                // NOP
            }

            throw;
        }
    }

    private Task ApplyAsync(IStreamSource source, IActionContext actionContext, CancellationToken cancellationToken)
    {
        _logger.Information("Renaming stream...");

        var oldName = source.Name;
        source.Name = Format(_template, oldName);
        
        _logger.Information("{Src} -> {Dst}...", oldName, source.Name);
        
        actionContext.SetResult(source);

        return Task.CompletedTask;
    }
    
    public RenameAction(ILogger logger, IFileSystem fs, string template, bool overwrite)
    {
        _logger = logger.ForContextShort<RenameAction>();
        _fs = fs;
        _template = template;
        _overwrite = overwrite;
    }

    public Task ApplyAsync(IActionContext actionContext, CancellationToken cancellationToken)
    {
        return ActionContextExecutor
            .For(actionContext)
            .ApplyFileListSource(ApplyAsync)
            .ApplyStreamSource(ApplyAsync)
            .ExecuteAsync(cancellationToken);
    }
}