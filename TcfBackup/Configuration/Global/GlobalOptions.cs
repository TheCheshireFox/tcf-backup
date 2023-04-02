using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Global;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class GlobalOptions
{
    public string Name { get; set; } = null!;
    public string? WorkingDir { get; set; }
    public RetentionOptions? Retention { get; set; }
}