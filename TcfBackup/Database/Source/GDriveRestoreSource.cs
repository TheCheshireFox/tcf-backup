namespace TcfBackup.Database.Source;

public class GDriveRestoreSource : IRestoreSource
{
    public RestoreSourceType Type => RestoreSourceType.GDrive;
    public string Path { get; set; }
}