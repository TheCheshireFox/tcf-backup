using LiteDB;
using TcfBackup.Filesystem;

namespace TcfBackup.BackupDatabase;

public class BackupRepository : IBackupRepository
{
    private const string DatabaseFileName = "backups.db";

    private readonly string _dbPath;

    public BackupRepository(IFileSystem fs, string directory)
    {
        fs.Directory.CreateDirectory(directory);
        _dbPath = fs.Path.Combine(directory, DatabaseFileName);
    }
    
    public IEnumerable<Backup> GetBackups()
    {
        using var db = new LiteDatabase(_dbPath);
        foreach (var backup in db.GetCollection<Backup>().FindAll())
        {
            yield return backup;
        }
    }

    public void AddBackup(Backup backup)
    {
        using var db = new LiteDatabase(_dbPath);
        
        var backups = db.GetCollection<Backup>();
        backups.EnsureIndex(x => x.Id, true);
        backups.Insert(backup);
    }

    public void DeleteBackup(Backup backup)
    {
        using var db = new LiteDatabase(_dbPath);
        
        var backups = db.GetCollection<Backup>();
        backups.DeleteMany(b => b.Id == backup.Id);

        db.Rebuild();
    }
}