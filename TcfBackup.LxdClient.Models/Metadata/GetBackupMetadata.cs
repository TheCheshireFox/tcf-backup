using Newtonsoft.Json;

namespace TcfBackup.LxdClient.Models.Metadata;

public class GetBackupMetadata
{
    [JsonProperty("container_only")]
    public bool ContainerOnly { get; set; }
    
    [JsonProperty("expires_at")]
    public DateTime ExpiresAt { get; set; }
    
    [JsonProperty("instance_only")]
    public bool InstanceOnly { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; } = null!;
    
    [JsonProperty("optimized_storage")]
    public bool OptimizedStorage { get; set; }
}