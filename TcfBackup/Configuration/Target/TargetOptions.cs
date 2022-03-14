namespace TcfBackup.Configuration.Target;

public enum TargetType
{
    None,
    Directory,
    GDrive
}

public class TargetOptions
{
    public TargetType Type { get; set; }
}