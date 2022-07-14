using TcfBackup.Shared;

namespace TcfBackup.Managers;

public class BtrfsManager : IBtrfsManager
{
    public void CheckAvailable()
    {
        if (SystemUtils.Which("btrfs") == null)
        {
            throw new FileNotFoundException("Btrfs executable not found");
        }
    }

    public void CreateSnapshot(string subvolume, string targetDir) => Subprocess.Exec("btrfs", $"subvolume snapshot -r {subvolume} {targetDir}");
    public void DeleteSubvolume(string subvolume) => Subprocess.Exec("btrfs", $"subvolume delete {subvolume}");
}