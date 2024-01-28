namespace TcfBackup.Retention.BackupCleaners;

public interface IBackupCleaner
{
    Task DeleteAsync(string path, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken);
}