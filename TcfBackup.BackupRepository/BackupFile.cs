namespace TcfBackup.BackupDatabase;

public class BackupFile
{
    public BackupFile()
    {
    }

    public BackupFile(string path)
    {
        Path = path;
    }

    public string Path { get; set; } = null!;
}