namespace TcfBackup.BackupDatabase;

public interface IBackupRepository
{
    IEnumerable<Backup> GetBackups();
    void AddBackup(Backup backup);
    void DeleteBackup(Backup backup);
}