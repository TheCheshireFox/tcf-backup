using System.Text.RegularExpressions;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action;

public class FilterAction : IAction
{
    private readonly IFileSystem _fs;
    private readonly bool _followSymlinks;
    private readonly Regex? _includeRegex;
    private readonly Regex? _excludeRegex;

    private static IEnumerable<string> PreprocessRegex(IEnumerable<string> regex)
    {
        return regex
            .Select(r => !r.Contains('^') ? "^" + r : r)
            .Select(r => !r.Contains('$') ? r + "$" : r)
            .Select(r => $"(?:{r})");
    }

    public FilterAction(ILogger logger, IFileSystem fs, string[] includeRegex, string[] excludeRegex, bool followSymlinks)
    {
        logger.ForContextShort<FilterAction>().Information("Filter initialized with regexes: {IncludeRe} {ExcludeRe}", includeRegex, excludeRegex);
        _includeRegex = includeRegex.Length > 0 ? new Regex(string.Join('|', PreprocessRegex(includeRegex))) : null;
        _excludeRegex = excludeRegex.Length > 0 ? new Regex(string.Join('|', PreprocessRegex(excludeRegex))) : null;
        _fs = fs;
        _followSymlinks = followSymlinks;
    }

    public ISource Apply(ISource source, CancellationToken cancellationToken)
    {
        var files = source.GetFiles(_followSymlinks);

        files = (_includeRegex, _excludeRegex) switch
        {
            (null, null) => files,
            (not null, null) => files.AsParallel().WithCancellation(cancellationToken).Where(file => _includeRegex.IsMatch(file.Path)),
            (null, not null) => files.AsParallel().WithCancellation(cancellationToken).Where(file => !_excludeRegex.IsMatch(file.Path)),
            (not null, not null) => files.AsParallel().WithCancellation(cancellationToken).Where(file => !_excludeRegex.IsMatch(file.Path) || _includeRegex.IsMatch(file.Path))
        };

        return new FilesListSource(_fs, files);
    }
}