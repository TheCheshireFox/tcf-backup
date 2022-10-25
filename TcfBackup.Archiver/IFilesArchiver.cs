namespace TcfBackup.Archiver;

public interface IFilesArchiver : IArchiver
{
    void AddEntry(string path);
}