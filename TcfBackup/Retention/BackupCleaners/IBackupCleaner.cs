using System.Threading.Tasks;

namespace TcfBackup.Retention.BackupCleaners;

public interface IBackupCleaner
{
    Task DeleteAsync(string path, bool throwIfNotExisted = false);
}