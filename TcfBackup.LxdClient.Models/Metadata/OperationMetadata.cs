using Newtonsoft.Json;

namespace TcfBackup.LxdClient.Models.Metadata;

public class OperationMetadata<T>
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    
    [JsonProperty("class")]
    public string Class { get; set; } = null!;
    
    [JsonProperty("description")]
    public string Description { get; set; } = null!;
    
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; } = null!;
    
    [JsonProperty("status_code")]
    public int StatusCode { get; set; }
    
    // [JsonProperty("resources")]
    // public List<object> Resources { get; set; }
    
    [JsonProperty("may_cancel")]
    public bool MayCancel { get; set; }
    
    [JsonProperty("err")]
    public string Err { get; set; } = null!;
    
    [JsonProperty("location")]
    public string Location { get; set; } = null!;
    
    [JsonProperty("metadata")]
    public T? Metadata { get; set; }
}