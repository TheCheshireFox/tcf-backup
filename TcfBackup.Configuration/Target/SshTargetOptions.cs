namespace TcfBackup.Configuration.Target;

public class SshTargetOptions : TargetOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? KeyFile { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool Overwrite { get; set; }
}