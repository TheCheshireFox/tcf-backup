namespace TcfBackup.Configuration.Action;

public class FilterActionOptions : ActionOptions
{
    public bool FollowSymlinks { get; set; }
    public string[] Include { get; set; } = Array.Empty<string>();
    public string[] Exclude { get; set; } = Array.Empty<string>();
}