namespace TcfBackup.Retention.BackupCleaners;

public interface IBackupCleanerFactory
{
    IBackupCleaner GetByScheme(string scheme);
}