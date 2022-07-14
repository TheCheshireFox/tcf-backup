namespace TcfBackup.Managers;

public interface IBtrfsManager : IManager
{
    void CreateSnapshot(string subvolume, string targetDir);
    void DeleteSubvolume(string subvolume);
}