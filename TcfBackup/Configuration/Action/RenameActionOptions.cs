using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Action;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class RenameActionOptions : ActionOptions
{
    public string Template { get; set; } = string.Empty;
    public bool Overwrite { get; set; }
}