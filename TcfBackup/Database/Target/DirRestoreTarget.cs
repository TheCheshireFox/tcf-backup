namespace TcfBackup.Database.Target;

public class DirRestoreTarget : IRestoreTarget
{
    public RestoreTargetType Type => RestoreTargetType.Directory;
    public string Path { get; set; }
}