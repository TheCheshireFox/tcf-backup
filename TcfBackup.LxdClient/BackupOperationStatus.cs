namespace TcfBackup.LxdClient;

public enum BackupOperationState
{
    InProgress,
    Complete,
    Error
}

public record BackupOperationStatus(string Progress, BackupOperationState State);