using System.Text;
using Newtonsoft.Json;

namespace TcfBackup.LxdClient;

internal static class HttpClientExtensions
{
    public static async Task<T?> GetAsync<T>(this HttpClient client, string url)
    {
        var response = await client.GetStringAsync(url);
        return JsonConvert.DeserializeObject<T>(response);
    }

    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(this HttpClient client, string url, TRequest payload)
    {
        using var response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TResponse>(content);
    }
}