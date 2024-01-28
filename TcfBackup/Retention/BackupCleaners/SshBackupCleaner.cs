using TcfBackup.Managers;

namespace TcfBackup.Retention.BackupCleaners;

public class SshBackupCleaner : IBackupCleaner
{
    private readonly ISshManager _sshManager;

    public SshBackupCleaner(ISshManager sshManager)
    {
        _sshManager = sshManager;
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        _sshManager.Delete(path, cancellationToken);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
    {
        return Task.FromResult(_sshManager.Exists(path, cancellationToken));
    }
}