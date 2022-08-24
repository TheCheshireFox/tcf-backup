namespace TcfBackup.Restore;

public class DecryptRestoreActionInfo : IRestoreActionInfo
{
    public string? KeyFile { get; init; }
    public string? Signature { get; init; }
    public string? Password { get; init; }
}