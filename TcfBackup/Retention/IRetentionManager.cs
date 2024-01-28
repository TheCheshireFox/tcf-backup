namespace TcfBackup.Retention;

public interface IRetentionManager
{
    Task PerformCleanupAsync(CancellationToken cancellationToken);
}