using System.Threading;
using System.Threading.Tasks;

namespace TcfBackup.Retention.BackupCleaners;

public class GDriveBackupCleaner : IBackupCleaner
{
    private readonly IGDriveAdapter _gDriveAdapter;

    public GDriveBackupCleaner(IGDriveAdapter gDriveAdapter)
    {
        _gDriveAdapter = gDriveAdapter;
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        await _gDriveAdapter.DeleteFileAsync(path, cancellationToken);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
    {
        return _gDriveAdapter.ExistsAsync(path, cancellationToken);
    }
}