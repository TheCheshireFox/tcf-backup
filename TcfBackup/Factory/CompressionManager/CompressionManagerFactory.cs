using Serilog;
using TcfBackup.Configuration.Action.CompressAction;
using TcfBackup.Managers;

namespace TcfBackup.Factory.CompressionManager;

public interface ICompressionManagerFactory
{
    ICompressionManager Create(CompressActionOptions opts);
}

public class CompressionManagerFactory : ICompressionManagerFactory
{
    private readonly ILogger _logger;

    public CompressionManagerFactory(ILogger logger)
    {
        _logger = logger;
    }

    public ICompressionManager Create(CompressActionOptions opts)
    {
        return opts switch
        {
            TarCompressActionOptions tarOptions => CreateTarCompressionManager(tarOptions),
            _ => throw new NotSupportedException($"Compression engine {opts.Engine} not supported")
        };
    }
    
    private Managers.CompressionManager CreateTarCompressionManager(TarCompressActionOptions opts)
    {
        var factory = TarArchiverFactory.Create(opts);
        var tarCompressionAlgorithm = opts.Compressor switch
        {
            TarCompressor.Gzip => CompressAlgorithm.Gzip,
            TarCompressor.Xz => CompressAlgorithm.Xz,
            TarCompressor.BZip2 => CompressAlgorithm.BZip2,
            _ => throw new NotSupportedException($"Compression algorithm {opts.Compressor} not supported")
        };
        
        return new Managers.CompressionManager(_logger, factory, tarCompressionAlgorithm);
    }
}