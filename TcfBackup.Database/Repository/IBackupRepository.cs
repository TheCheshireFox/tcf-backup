namespace TcfBackup.Database.Repository;

public interface IBackupRepository
{
    Task<IEnumerable<Backup>> GetBackupsAsync();
    Task AddBackupAsync(Backup backup, CancellationToken cancellationToken);
    Task DeleteBackupAsync(Backup backup, CancellationToken cancellationToken);
}