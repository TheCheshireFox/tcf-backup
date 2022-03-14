using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;

namespace TcfBackup.Source
{
    public class BtrfsSource : ISource, ISymlinkFilterable
    {
        private readonly ILogger _logger;
        private readonly IBtrfsManager _btrfsManager;
        private readonly IFilesystem _filesystem;
        private readonly string _subvolume;
        private readonly string? _snapshot;

        public BtrfsSource(ILogger logger, IBtrfsManager btrfsManager, IFilesystem filesystem, string subvolume, string? snapshotDir)
        {
            _logger = logger.ForContextShort<BtrfsSource>();
            _btrfsManager = btrfsManager;
            _filesystem = filesystem;
            _subvolume = subvolume;

            if (!_filesystem.DirectoryExists(subvolume)) throw new DirectoryNotFoundException(subvolume);
            if (snapshotDir == null) return;

            _snapshot = filesystem.DirectoryExists(snapshotDir)
                ? Path.Combine(snapshotDir, new DirectoryInfo(subvolume).Name)
                : snapshotDir;
        }

        public IEnumerable<IFile> GetFiles() => GetFiles(false);
        public IEnumerable<IFile> GetFiles(bool followSymlinks) => _filesystem.GetFiles(_snapshot ?? _subvolume, followSymlinks: followSymlinks).Select(f => (IFile)new ImmutableFile(_filesystem, f));

        public void Prepare()
        {
            if (_snapshot == null)
            {
                return;
            }
            
            _logger.Information("Creating snapshot of {subvolume} to {snapshot}", _subvolume, _snapshot);
            
            _btrfsManager.CreateSnapshot(_subvolume, _snapshot);
            
            _logger.Information("Snapshot created");
        }

        public void Cleanup()
        {
            if (_snapshot == null)
            {
                return;
            }
            
            _logger.Information("Deleting snapshot {snapshot}...", _snapshot);
            
            _btrfsManager.DeleteSubvolume(_snapshot);
            
            _logger.Information("Complete");
        }
    }
}