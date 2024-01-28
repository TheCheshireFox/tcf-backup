namespace TcfBackup.Configuration.Action.CompressAction;

public enum CompressEngine
{
    Tar
}

public class CompressActionOptions : ActionOptions
{
    [Variant<TarCompressActionOptions>(CompressEngine.Tar)]
    public CompressEngine Engine { get; set; }
    public string? Name { get; set; }
    public bool FollowSymlinks { get; set; }
    public string ChangeDir { get; set; } = string.Empty;
}