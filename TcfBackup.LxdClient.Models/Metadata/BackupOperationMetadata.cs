using Newtonsoft.Json;

namespace TcfBackup.LxdClient.Models.Metadata;

public class BackupOperationMetadata
{
    [JsonProperty("create_backup_progress")]
    public string CreateBackupProgress { get; set; } = string.Empty;
}