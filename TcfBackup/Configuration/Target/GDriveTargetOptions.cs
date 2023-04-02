using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Target;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class GDriveTargetOptions : TargetOptions
{
    public string? Path { get; set; }
}