using LinqToDB;
using LinqToDB.Data;

namespace TcfBackup.Database.Repository;

public class BackupRepository : IBackupRepository
{
    public async Task<IEnumerable<Backup>> GetBackupsAsync()
    {
        await using var db = new BackupDb();

        return (await db.Backup
                .LoadWith(b => b.Files)
                .ToListAsync())
            .Select(b => new Backup
            {
                Id = b.Id,
                Name = b.Name,
                Date = b.Date,
                Files = b.Files.Select(f => new BackupFile
                {
                    Path = f.Path
                })
            });
    }

    public async Task AddBackupAsync(Backup backup, CancellationToken cancellationToken)
    {
        await using var db = new BackupDb();
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        var dbBackup = new TcfBackup.Database.Backup
        {
            Name = backup.Name,
            Date = backup.Date
        };

        dbBackup.Id = await db.InsertWithInt32IdentityAsync(dbBackup, token: cancellationToken);

        var dbBackupFiles = backup.Files.Select(f => new TcfBackup.Database.BackupFile
        {
            BackupId = dbBackup.Id,
            Path = f.Path
        });
        
        await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = false,
            CheckConstraints = true
        }, dbBackupFiles, cancellationToken: cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteBackupAsync(Backup backup, CancellationToken cancellationToken)
    {
        await using var db = new BackupDb();
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        var dbBackup = await db.Backup.FirstAsync(b => b.Id == backup.Id, token: cancellationToken);
        
        await db.BackupFiles.DeleteAsync(f => f.BackupId == dbBackup.Id, token: cancellationToken);
        await db.Backup.DeleteAsync(b => b.Id == dbBackup.Id, token: cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
    }
}