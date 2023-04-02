using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Action;

public enum CompressEngine
{
    Tar
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class CompressActionOptions : ActionOptions
{
    public CompressEngine Engine { get; set; }
    public string? Name { get; set; }
    public bool FollowSymlinks { get; set; }
    public string ChangeDir { get; set; } = string.Empty;
}