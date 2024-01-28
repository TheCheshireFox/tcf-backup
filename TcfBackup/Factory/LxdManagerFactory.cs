using Serilog;
using TcfBackup.Configuration.Source;
using TcfBackup.Managers;
using TcfBackup.Shared.ProgressLogger;

namespace TcfBackup.Factory;

public interface ILxdManagerFactory
{
    ILxdManager Create(LxdSourceOptions opts);
}

public class LxdManagerFactory : ILxdManagerFactory
{
    private readonly ILogger _logger;
    private readonly IProgressLoggerFactory _progressLoggerFactory;

    public LxdManagerFactory(ILogger logger, IProgressLoggerFactory progressLoggerFactory)
    {
        _logger = logger;
        _progressLoggerFactory = progressLoggerFactory;
    }

    public ILxdManager Create(LxdSourceOptions opts) => new LxdManager(_logger, _progressLoggerFactory, opts.Address);
}