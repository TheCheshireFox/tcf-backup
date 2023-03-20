namespace TcfBackup.Archiver;

public interface IFilesArchiver : IDisposable
{
    void AddFile(string path);
}