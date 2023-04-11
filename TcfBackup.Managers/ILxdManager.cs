namespace TcfBackup.Managers;

public interface ILxdManager
{
    string[] ListContainers();
    void BackupContainer(string container, string targetFile, CancellationToken cancellationToken);
}