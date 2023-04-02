using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Global;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class RetentionOptions
{
    public string? Schedule { get; set; }
}