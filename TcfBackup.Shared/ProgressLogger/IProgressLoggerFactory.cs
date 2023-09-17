namespace TcfBackup.Shared.ProgressLogger;

public interface IProgressLoggerFactory
{
    IProgressLogger Create(long threshold);
}