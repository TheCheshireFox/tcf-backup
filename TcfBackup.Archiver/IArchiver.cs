namespace TcfBackup.Archiver;

public interface IArchiver : IDisposable
{
    void AddEntry(string path);
}