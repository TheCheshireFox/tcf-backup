namespace TcfBackup.Archiver;

public enum LogLevel
{
    Warning,
    Error
}

public interface IFilesArchiver : IDisposable
{
    event Action<LogLevel, string>? OnLog;
    
    void AddFile(string path, CancellationToken cancellationToken);
}