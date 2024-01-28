namespace TcfBackup.Configuration.Source;

public class LxdSourceOptions : SourceOptions
{
    public string Address { get; set; } = "unix:///var/lib/lxd/unix.socket";
    public string[] Containers { get; set; } = Array.Empty<string>();
    public bool IgnoreMissing { get; set; } = false;
}