using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Source;

public enum SourceType
{
    None,
    Btrfs,
    Directory,
    Lxd
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class SourceOptions
{
    public SourceType Type { get; set; }
}