using Newtonsoft.Json;

namespace TcfBackup.LxdClient.Models.Responses;

public abstract class BaseResponse<TMetadata>
{
    [JsonProperty("type")]
    public string Type { get; set; } = null!;
    
    [JsonProperty("status")]
    public string Status { get; set; } = null!;
    
    [JsonProperty("status_code")]
    public int StatusCode { get; set; }
    
    [JsonProperty("operation")]
    public string Operation { get; set; } = null!;
    
    [JsonProperty("error_code")]
    public int ErrorCode { get; set; }
    
    [JsonProperty("error")]
    public string Error { get; set; } = null!;
    
    [JsonProperty("metadata")]
    public TMetadata? Metadata { get; set; }
}