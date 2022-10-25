namespace TcfBackup.LxdClient.Operation;

public class LxdBackupOperation : LxdOperation
{
    internal string Container { get; }
    internal string BackupName { get; }
    
    public LxdBackupOperation(string operationUrl, string container, string backupName) : base(operationUrl)
    {
        Container = container;
        BackupName = backupName;
    }
}