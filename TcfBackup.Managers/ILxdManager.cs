namespace TcfBackup.Managers;

public interface ILxdManager : IManager
{
    string[] ListContainers();
    void BackupContainer(string container, string targetFile);
}