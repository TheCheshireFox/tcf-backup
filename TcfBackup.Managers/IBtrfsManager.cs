namespace TcfBackup.Managers;

public interface IBtrfsManager
{
    void CreateSnapshot(string subvolume, string targetDir, bool replace = false);
    void DeleteSubvolume(string subvolume);
}