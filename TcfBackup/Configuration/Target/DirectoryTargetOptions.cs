using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Target;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class DirectoryTargetOptions : TargetOptions
{
    public string Path { get; set; } = string.Empty;
    public bool Overwrite { get; set; }
}