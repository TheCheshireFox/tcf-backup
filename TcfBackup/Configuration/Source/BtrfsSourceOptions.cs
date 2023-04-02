using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Source;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class BtrfsSourceOptions : SourceOptions
{
    public string Subvolume { get; set; } = string.Empty;
    public string SnapshotDir { get; set; } = string.Empty;
}