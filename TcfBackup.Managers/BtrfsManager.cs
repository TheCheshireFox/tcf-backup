using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Managers;

public class BtrfsManager : IBtrfsManager
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fs;
    private readonly BtrfsUtil.BtrfsUtil _btrfsUtil = new();

    public BtrfsManager(ILogger logger, IFileSystem fs)
    {
        _logger = logger.ForContextShort<BtrfsManager>();
        _fs = fs;
    }
    
    private void TryDeleteOldSnapshot(string subvolume, string targetDir)
    {
        if (!_fs.Directory.Exists(targetDir) || !_btrfsUtil.IsSubvolume(targetDir))
        {
            return;
        }

        var targetInfo = _btrfsUtil.GetSubvolumeInfo(targetDir);
        if (targetInfo.ParentUuid == Guid.Empty)
        {
            throw new Exception($"Subvolume {targetDir} is not snapshot");
        }
        
        var subvolInfo = _btrfsUtil.GetSubvolumeInfo(subvolume);
        if (targetInfo.ParentUuid != subvolInfo.Uuid)
        {
            throw new Exception($"Subvolume {targetDir} is not snapshot of {subvolume}");
        }

        _logger.Information("Deleting old snapshot {SnapshotDir}", targetDir);
        _btrfsUtil.DeleteSubvolume(targetDir);
    }

    public void CreateSnapshot(string subvolume, string targetDir, bool replace = false)
    {
        if (replace)
        {
            TryDeleteOldSnapshot(subvolume, targetDir);
        }

        _btrfsUtil.CreateSnapshot(subvolume, targetDir);
    }

    public void DeleteSubvolume(string subvolume)
    {
        _btrfsUtil.DeleteSubvolume(subvolume);
    }
}