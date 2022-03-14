using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action
{
    public class CompressAction : IAction
    {
        private readonly ILogger _logger;
        private readonly IFilesystem _filesystem;
        private readonly ICompressionManager _compressionManager;
        private readonly CompressAlgorithm _compressAlgorithm;
        private readonly string? _archiveName;
        private readonly string _changeDir;
        private readonly string? _transform;
        private readonly bool _followSymlinks;

        private string AlgorithmToExtension()
        {
            return _compressAlgorithm switch
            {
                CompressAlgorithm.Gzip => ".tar.gz",
                CompressAlgorithm.Lzma => ".tar.lzma",
                CompressAlgorithm.Lzop => ".tar.lzop",
                CompressAlgorithm.Xz => ".tar.xz",
                CompressAlgorithm.BZip2 => ".tar.bz",
                CompressAlgorithm.LZip => ".tar.lz",
                CompressAlgorithm.ZStd => ".tar.zst",
                _ => throw new NotImplementedException()
            };
        }

        public CompressAction(ILogger logger, ICompressionManager compressionManager, IFilesystem filesystem, CompressAlgorithm algo, string? archiveName, string changeDir, string? transform, bool followSymlinks)
        {
            _logger = logger.ForContextShort<CompressAction>();
            _compressionManager = compressionManager;
            _filesystem = filesystem;
            _compressAlgorithm = algo;
            _archiveName = archiveName;
            _changeDir = changeDir;
            _transform = transform;
            _followSymlinks = followSymlinks;
        }

        public ISource Apply(ISource source)
        {
            var archiveName = string.IsNullOrEmpty(_archiveName)
                ? StringExtensions.GenerateRandomString(8) + AlgorithmToExtension()
                : string.IsNullOrEmpty(PathUtils.GetFullExtension(_archiveName))
                    ? _archiveName + AlgorithmToExtension()
                    : _archiveName;

            var archiveFile = _filesystem.CreateTempFile(archiveName, true);

            var files = source.GetFiles();

            _logger.Information("Compressing files with algorithm {algo}", _compressAlgorithm);
            _compressionManager.Compress(_compressAlgorithm, archiveFile, files.Select(f => f.Path).ToArray(), _changeDir, _followSymlinks);
            _logger.Information("Complete");

            return FilesListSource.CreateMutable(_filesystem, new[] { archiveFile });
        }
    }
}