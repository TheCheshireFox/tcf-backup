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
    public TargetType Type { get; set; }
}