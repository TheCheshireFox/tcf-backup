namespace TcfBackup.Database.Action;

public class DecryptRestoreAction : IRestoreAction
{
    public RestoreActionType Type => RestoreActionType.Decrypt;

    public string? Password { get; set; }
    public string? KeyFile { get; set; }
    public string? Signature { get; set; }
}