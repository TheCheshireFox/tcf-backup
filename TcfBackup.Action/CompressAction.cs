using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action;

public class CompressAction : IAction
{
    private readonly ILogger _logger;
    private readonly IFilesystem _filesystem;
    private readonly ICompressionManager _compressionManager;
    private readonly CompressAlgorithm[] _compressAlgorithms;
    private readonly string? _archiveName;
    private readonly string _changeDir;
    private readonly bool _followSymlinks;

    private string AlgorithmToExtension()
    {
        return string.Join('.', _compressAlgorithms.Select(algo => algo switch
        {
            CompressAlgorithm.Gzip => "gz",
            CompressAlgorithm.Xz => "xz",
            CompressAlgorithm.BZip2 => "bz",
            _ => throw new NotSupportedException($"Compression algorithm {algo} not supported")
        }).Prepend(".tar"));
    }

    public CompressAction(ILogger logger, ICompressionManager compressionManager, IFilesystem filesystem, CompressAlgorithm[] compressAlgorithms, string? archiveName, string changeDir, bool followSymlinks)
    {
        _logger = logger.ForContextShort<CompressAction>();
        _compressionManager = compressionManager;
        _filesystem = filesystem;
        _compressAlgorithms = compressAlgorithms;
        _archiveName = archiveName;
        _changeDir = changeDir;
        _followSymlinks = followSymlinks;
    }

    public ISource Apply(ISource source, CancellationToken cancellationToken)
    {
        var archiveName = string.IsNullOrEmpty(_archiveName)
            ? StringExtensions.GenerateRandomString(8) + AlgorithmToExtension()
            : string.IsNullOrEmpty(PathUtils.GetFullExtension(_archiveName))
                ? _archiveName + AlgorithmToExtension()
                : _archiveName;

        var archiveFile = _filesystem.CreateTempFile(archiveName, true);

        var files = source.GetFiles(_followSymlinks);

        _logger.Information("Compressing files with algorithm {algo}", _compressAlgorithms);
        _compressionManager.Compress(_compressAlgorithms, archiveFile, files.Select(f => f.Path).ToList(), _changeDir, _followSymlinks, cancellationToken);
        _logger.Information("Complete");

        return FilesListSource.CreateMutable(_filesystem, new[] { archiveFile });
    }
}