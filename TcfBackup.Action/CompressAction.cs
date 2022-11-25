using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action;

public class CompressAction : IAction
{
    private readonly ILogger _logger;
    private readonly IFileSystem _filesystem;
    private readonly ICompressionManager _compressionManager;
    private readonly string? _archiveName;
    private readonly string _changeDir;
    private readonly bool _followSymlinks;

    public CompressAction(ILogger logger, ICompressionManager compressionManager, IFileSystem filesystem, string? archiveName, string changeDir, bool followSymlinks)
    {
        _logger = logger.ForContextShort<CompressAction>();
        _compressionManager = compressionManager;
        _filesystem = filesystem;
        _archiveName = archiveName;
        _changeDir = changeDir;
        _followSymlinks = followSymlinks;
    }

    public ISource Apply(ISource source, CancellationToken cancellationToken)
    {
        var archiveName = string.IsNullOrEmpty(_archiveName)
            ? $"{StringExtensions.GenerateRandomString(8)}.{_compressionManager.FileExtension}"
            : string.IsNullOrEmpty(PathUtils.GetFullExtension(_archiveName))
                ? $"{_archiveName}.{_compressionManager.FileExtension}"
                : _archiveName;

        var archiveFile = _filesystem.Path.GetTempFileName(archiveName, true);

        var files = source.GetFiles(_followSymlinks);

        _logger.Information("Compressing files...");
        _compressionManager.Compress(archiveFile, files.Select(f => f.Path).ToList(), _changeDir, _followSymlinks, cancellationToken);
        _logger.Information("Complete");

        return FilesListSource.CreateMutable(_filesystem, new[] { archiveFile });
    }
}