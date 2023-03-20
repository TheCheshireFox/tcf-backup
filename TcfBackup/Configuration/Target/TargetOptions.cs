namespace TcfBackup.Configuration.Target;

public enum TargetType
{
    None,
    Dir,
    GDrive
}

public class TargetOptions
{
    public TargetType Type { get; set; }
}