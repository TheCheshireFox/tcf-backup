using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;

namespace TcfBackup.Factory;

public interface IBtrfsManagerFactory
{
    IBtrfsManager Create();
}

public class BtrfsManagerFactory : IBtrfsManagerFactory
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fs;

    public BtrfsManagerFactory(ILogger logger, IFileSystem fs)
    {
        _logger = logger;
        _fs = fs;
    }

    public IBtrfsManager Create() => new BtrfsManager(_logger, _fs);
}