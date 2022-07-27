namespace TcfBackup.Database.Target;

public interface IRestoreTarget
{
    RestoreTargetType Type { get; }
}