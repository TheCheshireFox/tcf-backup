using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
using IFile = TcfBackup.Filesystem.IFile;

namespace TcfBackup.Source;

public class BtrfsSource : ISource, ISymlinkFilterable
{
    private readonly ILogger _logger;
    private readonly IBtrfsManager _btrfsManager;
    private readonly IFileSystem _filesystem;
    private readonly string _subvolume;
    private readonly string? _snapshot;

    private static string GetSubvolumeName(string subvolume)
    {
        var directoryInfo = new DirectoryInfo(subvolume);
        return directoryInfo.Parent == null
            ? "root"
            : directoryInfo.Name;
    }
    
    public BtrfsSource(ILogger logger, IBtrfsManager btrfsManager, IFileSystem filesystem, string subvolume, string? snapshotDir)
    {
        _logger = logger.ForContextShort<BtrfsSource>();
        _btrfsManager = btrfsManager;
        _filesystem = filesystem;
        _subvolume = subvolume;

        if (!_filesystem.Directory.Exists(subvolume)) throw new DirectoryNotFoundException(subvolume);
        if (snapshotDir == null) return;

        _snapshot = filesystem.Directory.Exists(snapshotDir)
            ? Path.Combine(snapshotDir, GetSubvolumeName(subvolume))
            : filesystem.Directory.Exists(Path.GetDirectoryName(snapshotDir))
                ? snapshotDir
                : throw new DirectoryNotFoundException(snapshotDir);
    }

    public IEnumerable<IFile> GetFiles() => GetFiles(false);
    public IEnumerable<IFile> GetFiles(bool followSymlinks) => _filesystem.Directory
        .GetFiles(_snapshot ?? _subvolume, recursive: true, sameFilesystem: true, skipAccessDenied: true, followSymlinks)
        .Select(f => (IFile)new ImmutableFile(_filesystem, f));

    public void Prepare()
    {
        if (_snapshot == null)
        {
            return;
        }

        _logger.Information("Creating snapshot of {Subvolume} to {Snapshot}", _subvolume, _snapshot);

        _btrfsManager.CreateSnapshot(_subvolume, _snapshot);

        _logger.Information("Snapshot created");
    }

    public void Cleanup()
    {
        if (_snapshot == null)
        {
            return;
        }

        _logger.Information("Deleting snapshot {Snapshot}...", _snapshot);

        _btrfsManager.DeleteSubvolume(_snapshot);

        _logger.Information("Complete");
    }
}