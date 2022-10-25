namespace TcfBackup.Archiver;

public interface IArchiver : IDisposable
{
    Stream Output { get; }
}