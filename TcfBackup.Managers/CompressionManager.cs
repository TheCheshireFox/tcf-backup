using Serilog;
using TcfBackup.Archiver;
using TcfBackup.Shared;

namespace TcfBackup.Managers;

public enum CompressAlgorithm
{
    BZip2,
    Xz,
    Gzip
}

public interface IArchiverFactory
{
    IFilesArchiver CreateFilesArchiver(Stream archive);
}

public class CompressionManager : ICompressionManager
{
    private readonly ILogger _logger;
    private readonly IArchiverFactory _archiverFactory;

    public CompressAlgorithm CompressAlgorithm { get; }
    
    public CompressionManager(ILogger logger, IArchiverFactory archiverFactory, CompressAlgorithm compressAlgorithm)
    {
        _logger = logger.ForContextShort<CompressionManager>();
        _archiverFactory = archiverFactory;
        
        CompressAlgorithm = compressAlgorithm;
    }

    public void Compress(Stream archive, IEnumerable<string> files, CancellationToken cancellationToken = default)
    {
        using var archiver = _archiverFactory.CreateFilesArchiver(archive);
        
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                archiver.AddFile(file);
                _logger.Information("{File}", file);
            }
            catch (Exception e)
            {
                throw new IOException($"Unable to compress {file}", e);
            }
        }
    }

    public void CheckAvailable()
    {
        
    }
}