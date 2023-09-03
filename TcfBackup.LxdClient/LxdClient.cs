using System.Net.Sockets;
using TcfBackup.LxdClient.Models.Requests;
using TcfBackup.LxdClient.Models.Responses;
using TcfBackup.LxdClient.Operation;

namespace TcfBackup.LxdClient;

public class LxdClient
{
    private const string InstancesUrl = "/1.0/instances";
    
    private readonly string _address;
    private readonly Func<string, HttpClient> _connectionFactory;

    private static string GetUriWithoutScheme(string path)
    {
        return Uri.TryCreate(path, UriKind.Absolute, out var uri)
            ? uri.GetComponents(UriComponents.AbsoluteUri & ~ UriComponents.Scheme, UriFormat.UriEscaped)
            : throw new FormatException();
    }
    
    private static HttpClient ConnectUnixStream(string path) =>
        new(new SocketsHttpHandler
        {
            ConnectCallback = async (_, token) =>
            {
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                var endpoint = new UnixDomainSocketEndPoint(GetUriWithoutScheme(path));
                await socket.ConnectAsync(endpoint, token);
                return new NetworkStream(socket, ownsSocket: true);
            }
        })
        {
            BaseAddress = new Uri("http://localhost")
        };

    private static HttpClient ConnectUrl(string url) =>
        new()
        {
            BaseAddress = new Uri(url)
        };
    
    public LxdClient(string address)
    {
        _address = address;
        _connectionFactory = new Uri(_address).Scheme switch
        {
            "unix" => ConnectUnixStream,
            _ => ConnectUrl
        };
    }

    public void CheckAvailable()
    {
        using var _ = _connectionFactory(_address);
    }
    
    public async Task<string[]> ListContainersAsync()
    {
        using var httpClient = _connectionFactory(_address);
        var resp = await httpClient.GetAsync<InstancesResponse>(InstancesUrl);
        
        if (resp?.Metadata == null)
        {
            return Array.Empty<string>();
        }

        return resp.Metadata
            .Select(c => c.StartsWith(InstancesUrl) ? c[(InstancesUrl.Length + 1)..] : throw new FormatException($"Invalid instance url: {c}"))
            .ToArray();
    }

    public async Task<LxdBackupOperation> BackupContainerAsync(string container, string backupName, string compression, DateTime expiresAt)
    {
        using var httpClient = _connectionFactory(_address);
        var request = new StartBackupRequest
        {
            Name = backupName,
            CompressionAlgorithm = compression,
            ContainerOnly = false,
            InstanceOnly = true,
            OptimizedStorage = false,
            ExpiresAt = expiresAt
        };
        
        var resp = await httpClient.PostAsync<StartBackupRequest, StartBackupResponse>($"{InstancesUrl}/{container}/backups", request);
        var operation = resp?.Operation;
        if (operation == null)
        {
            throw new Exception();
        }

        return new LxdBackupOperation(operation, container, backupName);
    }

    public async Task<BackupOperationStatus> GetBackupOperationStatus(LxdBackupOperation operation)
    {
        using var httpClient = _connectionFactory(_address);
        var operationStatus = await httpClient.GetAsync<BackupOperationResponse>(operation.Url);

        var progress = operationStatus?.Metadata?.Metadata?.CreateBackupProgress ?? string.Empty;
        return operationStatus?.Metadata?.StatusCode switch
        {
            null => new BackupOperationStatus(progress, BackupOperationState.InProgress),
            103 => new BackupOperationStatus(progress, BackupOperationState.InProgress),
            200 => new BackupOperationStatus(progress, BackupOperationState.Complete),
            _ => new BackupOperationStatus(progress, BackupOperationState.Error)
        };
    }

    public async Task<Stream> DownloadBackup(LxdBackupOperation operation)
    {
        using var httpClient = _connectionFactory(_address);
        return await httpClient.GetStreamAsync($"{InstancesUrl}/{operation.Container}/backups/{operation.BackupName}/export");
    }

    public async Task DeleteBackup(LxdBackupOperation operation)
    {
        using var httpClient = _connectionFactory(_address);
        var resp = await httpClient.DeleteAsync($"{InstancesUrl}/{operation.Container}/backups/{operation.BackupName}");
        resp.EnsureSuccessStatusCode();
    }
}