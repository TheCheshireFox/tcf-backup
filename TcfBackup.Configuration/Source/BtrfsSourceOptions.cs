namespace TcfBackup.Configuration.Source;

public class BtrfsSourceOptions : SourceOptions
{
    public string Subvolume { get; set; } = string.Empty;
    public string SnapshotDir { get; set; } = string.Empty;
}