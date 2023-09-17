using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Target;

public enum TargetType
{
    None,
    Dir,
    GDrive,
    Ssh
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class TargetOptions
{
    public TargetType Type { get; set; }
}