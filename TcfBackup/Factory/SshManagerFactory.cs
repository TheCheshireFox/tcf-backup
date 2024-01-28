using Serilog;
using TcfBackup.Configuration.Target;
using TcfBackup.Managers;
using TcfBackup.Shared.ProgressLogger;

namespace TcfBackup.Factory;

public interface ISshManagerFactory
{
    ISshManager Create(SshTargetOptions options);
}

public class SshManagerFactory : ISshManagerFactory
{
    private readonly ILogger _logger;
    private readonly IProgressLoggerFactory _progressLoggerFactory;

    public SshManagerFactory(ILogger logger, IProgressLoggerFactory progressLoggerFactory)
    {
        _logger = logger;
        _progressLoggerFactory = progressLoggerFactory;
    }

    public ISshManager Create(SshTargetOptions options)
        => new SshManager(_logger, _progressLoggerFactory, options.Host, options.Username, options.Port, options.Password, options.KeyFile);
}