namespace TcfBackup.Database.Source;

public class DirRestoreSource : IRestoreSource
{
    public RestoreSourceType Type => RestoreSourceType.Directory;
    public string Path { get; set; }
}