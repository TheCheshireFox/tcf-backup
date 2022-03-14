using System.Text.RegularExpressions;
using Serilog;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action
{
    public class FilterAction : IAction
    {
        private readonly bool _followSymlinks;
        private readonly Regex? _includeRegex;
        private readonly Regex? _excludeRegex;

        public FilterAction(ILogger logger, string[] includeRegex, string[] excludeRegex, bool followSymlinks)
        {
            logger.ForContextShort<FilterAction>().Information("Filter initialized with regexes: {includeRe} {excludeRe}", includeRegex, excludeRegex);
            _includeRegex = includeRegex.Length > 0 ? new Regex(string.Join('|', includeRegex.Select(r => $"(?:{r})"))) : null;
            _excludeRegex = excludeRegex.Length > 0 ? new Regex(string.Join('|', excludeRegex.Select(r => $"(?:{r})"))) : null;
            _followSymlinks = followSymlinks;
        }
        
        public ISource Apply(ISource source)
        {
            var files = (source is ISymlinkFilterable symlinkFilterable
                    ? symlinkFilterable.GetFiles(_followSymlinks)
                    : source.GetFiles())
                .Where(file => _includeRegex != null && _includeRegex.IsMatch(file.Path) || _excludeRegex == null || !_excludeRegex.IsMatch(file.Path));
            
            return new FilesListSource(files);
        }
    }
}