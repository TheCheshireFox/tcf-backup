using Serilog;
using TcfBackup.LxdClient;
using TcfBackup.LxdClient.Operation;
using TcfBackup.Shared;

namespace TcfBackup.Managers;

public class LxdManager : ILxdManager
{
    private class BackupDeleter : IAsyncDisposable
    {
        private readonly LxdClient.LxdClient _lxdClient;
        private readonly LxdBackupOperation _operation;

        public BackupDeleter(LxdClient.LxdClient lxdClient, LxdBackupOperation operation)
        {
            _lxdClient = lxdClient;
            _operation = operation;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _lxdClient.DeleteBackup(_operation);
            }
            catch
            {
                // NOP
            }
        }
    }
    
    private readonly ILogger _logger;
    private readonly LxdClient.LxdClient _lxdClient;
    public LxdManager(ILogger logger, string address)
    {
        _logger = logger.ForContextShort<LxdManager>();
        _lxdClient = new LxdClient.LxdClient(address);
    }

    private async Task BackupContainerAsync(string container, string targetFile)
    {
        var backupName = $"tcf-backup.{container}.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var operation = await _lxdClient.BackupContainerAsync(container, backupName, "gzip", DateTime.Now.AddDays(1));

        await using var _ = new BackupDeleter(_lxdClient, operation);
        
        while (true)
        {
            await Task.Delay(1000);

            var status = await _lxdClient.GetBackupOperationStatus(operation);
            if (status.State == BackupOperationState.InProgress)
            {
                _logger.Information("Backup: {Progress}", status.Progress);
                continue;
            }

            if (status.State == BackupOperationState.Complete)
            {
                break;
            }

            throw new Exception();
        }

        var buffer = new byte[10 * 1024 * 1024];
        var progressLogger = new ProgressLogger(buffer.Length);
        progressLogger.OnProgress += bytes => _logger.Information("Transferred: {Bytes}", StringExtensions.FormatBytes(bytes));
        
        await using var backupStream = await _lxdClient.DownloadBackup(operation);
        await using var file = File.Open(targetFile, FileMode.Create, FileAccess.Write, FileShare.None);
        
        int count;
        while ((count = await backupStream.ReadAsync(buffer)) != 0)
        {
            progressLogger.Update(count);
            await file.WriteAsync(buffer.AsMemory()[..count]);
        }
    }
    
    public void CheckAvailable() => _lxdClient.CheckAvailable();

    public string[] ListContainers() => _lxdClient.ListContainersAsync().ConfigureAwait(false).GetAwaiter().GetResult();

    public void BackupContainer(string container, string targetFile) =>
        BackupContainerAsync(container, targetFile).ConfigureAwait(false).GetAwaiter().GetResult();
}