namespace TcfBackup.Configuration.Target;

public enum TargetType
{
    None,
    Dir,
    GDrive,
    Ssh
}

public class TargetOptions
{
    [Variant<DirectoryTargetOptions>(TargetType.Dir)]
    [Variant<GDriveTargetOptions>(TargetType.GDrive)]
    [Variant<SshTargetOptions>(TargetType.Ssh)]
    public TargetType Type { get; set; }
}