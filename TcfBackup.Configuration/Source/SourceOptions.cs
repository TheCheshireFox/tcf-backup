namespace TcfBackup.Configuration.Source;

public enum SourceType
{
    None,
    Btrfs,
    Directory,
    Lxd
}

public class SourceOptions
{
    [Variant<BtrfsSourceOptions>(SourceType.Btrfs)]
    [Variant<DirectorySourceOptions>(SourceType.Directory)]
    [Variant<LxdSourceOptions>(SourceType.Lxd)]
    public SourceType Type { get; set; }
}