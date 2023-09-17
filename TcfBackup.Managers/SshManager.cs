using Renci.SshNet;
using Serilog;
using TcfBackup.Shared;
using TcfBackup.Shared.ProgressLogger;

namespace TcfBackup.Managers;

public class SshManager : ISshManager, IDisposable
{
    private readonly ILogger _logger;
    private readonly IProgressLoggerFactory _progressLoggerFactory;
    private readonly SftpClient _sftpClient;

    public SshManager(ILogger logger, IProgressLoggerFactory progressLoggerFactory, string host, string username, int port = 22, string? password = null, string? keyFile = null)
    {
        _logger = logger.ForContextShort<SshManager>();
        _progressLoggerFactory = progressLoggerFactory;

        var authMethods = new List<AuthenticationMethod>();
        if (!string.IsNullOrEmpty(password)) authMethods.Add(new PasswordAuthenticationMethod(username, password));
        if (!string.IsNullOrEmpty(keyFile)) authMethods.Add(new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(keyFile)));

        _sftpClient = new SftpClient(new ConnectionInfo(host, port, username, authMethods.ToArray()));
        _sftpClient.Connect();
    }

    public void Upload(Stream src, string dst, bool overwrite, CancellationToken cancellationToken)
    {
        Connect();
        using var ctr = cancellationToken.Register(() => _sftpClient.Disconnect());
        
        var progressLogger = _progressLoggerFactory.Create(ProgressLogger.GetThreshold(src));
        progressLogger.OnProgress += transferred => _logger.Information("Transferred: {Total}", StringExtensions.FormatBytes(transferred));

        try
        {
            _sftpClient.UploadFile(src, dst, overwrite, uploaded => progressLogger.Set((long)uploaded));
        }
        catch (Exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }

    public void Delete(string path, CancellationToken cancellationToken)
    {
        Connect();
        using var ctr = cancellationToken.Register(() => _sftpClient.Disconnect());

        try
        {
            _sftpClient.DeleteFile(path);
        }
        catch (Exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }

    public bool Exists(string path, CancellationToken cancellationToken)
    {
        Connect();
        using var ctr = cancellationToken.Register(() => _sftpClient.Disconnect());

        try
        {
            return _sftpClient.Exists(path);
        }
        catch (Exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }

    public void Dispose()
    {
        _sftpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Connect()
    {
        if (_sftpClient.IsConnected)
        {
            return;
        }

        var connInfo = _sftpClient.ConnectionInfo;
        _logger.Information("Connecting to {User}@{Host}:{Port}...", connInfo.Username, connInfo.Host, connInfo.Port);
        _sftpClient.Connect();
        _logger.Information("Connected");
    }
}