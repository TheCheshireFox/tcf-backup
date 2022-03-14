using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;

namespace TcfBackup.Source
{
    public class LxdSnapshotSource : ISource
    {
        private readonly ILogger _logger;
        private readonly ILxdManager _lxdManager;
        private readonly IFilesystem _filesystem;
        private readonly string[] _containers;

        private string? _backupDirectory;

        public LxdSnapshotSource(ILogger logger, ILxdManager lxdManager, IFilesystem filesystem, string[] containers, bool ignoreNonExisted)
        {
            _logger = logger.ForContextShort<LxdSnapshotSource>();
            _lxdManager = lxdManager;
            _filesystem = filesystem;

            var lxdContainers = _lxdManager.ListContainers();
            _logger.Information("Lxd containers: {containers}", lxdContainers);

            if (containers.Length == 0)
            {
                _containers = lxdContainers;
            }
            else
            {
                var missed = containers.Except(lxdContainers).ToArray();

                if (missed.Length > 0)
                {
                    _containers = ignoreNonExisted
                        ? containers.Except(missed).ToArray()
                        : throw new Exception($"Lxd containers missed: {string.Join(", ", missed)}");
                }
                else
                {
                    _containers = containers;
                }
            }

            _logger.Information("Backup containers: {containers}", _containers);
        }
        
        public IEnumerable<IFile> GetFiles() => _filesystem
            .GetFiles(_backupDirectory ?? throw new InvalidOperationException("Unable to get backup archives: No backup was performed"))
            .Select(f => (IFile)new MutableFile(_filesystem, f));

        public void Prepare()
        {
            _backupDirectory = _filesystem.CreateTempDirectory();

            try
            {
                _logger.Information("Preparing snapshots...");
                foreach (var container in _containers)
                {
                    _logger.Information("Creating snapshot for container {container}", container);
                    
                    _lxdManager.BackupContainer(container, Path.Combine(_backupDirectory, container + ".tar.gz"));
                    
                    _logger.Information("Snapshot created");
                }
            }
            catch (Exception)
            {
                Cleanup();
            }
        }

        public void Cleanup()
        {
            try
            {
                if (_filesystem.DirectoryExists(_backupDirectory))
                {
                    _filesystem.Delete(_backupDirectory!);
                }
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }
}