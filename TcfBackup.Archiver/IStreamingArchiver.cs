namespace TcfBackup.Archiver;

public interface IStreamingArchiver : IArchiver
{
    Stream Input { get; }
}