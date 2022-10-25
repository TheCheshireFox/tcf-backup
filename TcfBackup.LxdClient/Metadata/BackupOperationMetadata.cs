using Newtonsoft.Json;

namespace TcfBackup.LxdClient.Metadata;

internal class BackupOperationMetadata
{
    [JsonProperty("create_backup_progress")]
    public string CreateBackupProgress { get; set; } = string.Empty;
}