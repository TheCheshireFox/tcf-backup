using System;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;

namespace TcfBackup.Factory.Manager.Compression;

public class CompressionManagerFactoryScoped : ManagerFactoryScoped<ICompressionManager, CompressionManagerType>
{
    private readonly IFilesystem _fs;
    private readonly ILogger _logger;

    public CompressionManagerFactoryScoped(IFilesystem fs, ILogger logger)
    {
        _fs = fs;
        _logger = logger;
    }

    public ICompressionManager Create(CompressionManagerType selector) => selector switch
    {
        CompressionManagerType.TarExecutable => new TarCompressionManager(_logger, _fs),
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, null)
    };
}