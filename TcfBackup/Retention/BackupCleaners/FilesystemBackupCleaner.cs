using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TcfBackup.Filesystem;

namespace TcfBackup.Retention.BackupCleaners;

public class FilesystemBackupCleaner : IBackupCleaner
{
    private readonly IFileSystem _fs;

    public FilesystemBackupCleaner(IFileSystem fs)
    {
        _fs = fs;
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        _fs.File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
    {
        return Task.FromResult(_fs.File.Exists(path));
    }
}