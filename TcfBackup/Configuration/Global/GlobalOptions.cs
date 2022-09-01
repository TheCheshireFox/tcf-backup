namespace TcfBackup.Configuration.Global;

public class GlobalOptions
{
    public string Name { get; set; } = null!;
    public string? WorkingDir { get; set; }
    public RetentionOptions? Retention { get; set; }
}