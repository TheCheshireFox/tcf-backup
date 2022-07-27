namespace TcfBackup.Database.Action;

public class DecompressRestoreAction : IRestoreAction
{
    public RestoreActionType Type => RestoreActionType.Decompress;
}