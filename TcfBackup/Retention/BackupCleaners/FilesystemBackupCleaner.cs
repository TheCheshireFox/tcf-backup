using System.IO;
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

    public Task DeleteAsync(string path, bool throwIfNotExisted = false)
    {
        try
        {
            _fs.File.Delete(path);
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