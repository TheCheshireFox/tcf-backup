namespace TcfBackup.Managers
{
    public interface IBtrfsManager
    {
        void CreateSnapshot(string subvolume, string targetDir);
        void DeleteSubvolume(string subvolume);
    }
}