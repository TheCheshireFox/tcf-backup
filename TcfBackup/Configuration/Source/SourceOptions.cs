namespace TcfBackup.Configuration.Source
{
    public enum SourceType
    {
        None,
        Btrfs,
        Directory,
        Lxd
    }
    
    public class SourceOptions
    {
        public SourceType Type { get; set; }
    }
}