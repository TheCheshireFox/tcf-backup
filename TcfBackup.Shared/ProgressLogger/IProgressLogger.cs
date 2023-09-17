namespace TcfBackup.Shared.ProgressLogger;

public interface IProgressLogger
{
    event Action<long>? OnProgress;
    void Set(long value);
    void Add(long value);
}