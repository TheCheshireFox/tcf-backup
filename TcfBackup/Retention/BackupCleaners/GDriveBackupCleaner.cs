using System.IO;
using System.Threading.Tasks;

namespace TcfBackup.Retention.BackupCleaners;

public class GDriveBackupCleaner : IBackupCleaner
{
    private readonly IGDriveAdapter _gDriveAdapter;

    public GDriveBackupCleaner(IGDriveAdapter gDriveAdapter)
    {
        _gDriveAdapter = gDriveAdapter;
    }

    public Task DeleteAsync(string path, bool throwIfNotExisted = false)
    {
        try
        {
            _gDriveAdapter.DeleteFile(path);
        }
        catch (FileNotFoundException)
        {
            if (throwIfNotExisted)
            {
                throw;
            }
        }
        
        return Task.CompletedTask;
    }
}