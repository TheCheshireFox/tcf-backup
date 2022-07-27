namespace TcfBackup.Database.Source;

public interface IRestoreSource
{
    RestoreSourceType Type { get; }
}