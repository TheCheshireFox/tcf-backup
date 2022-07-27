namespace TcfBackup.Database.Action;

public interface IRestoreAction
{
    RestoreActionType Type { get; }
}