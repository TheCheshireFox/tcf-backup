using System.Runtime.InteropServices;
using Serilog;
using TcfBackup.Archiver.Archivers;
using TcfBackup.Compressor;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Managers;

public enum CompressAlgorithm
{
    BZip2,
    Xz,
    Gzip
}

public interface ITarCompressionManagerCompressorFactory
{
    Stream Create(Stream output);
}

public class TarCompressionManager : ICompressionManager
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fs;
    private readonly ITarCompressionManagerCompressorFactory _compressorStreamFactory;

    public TarCompressionManager(ILogger logger, IFileSystem fs, ITarCompressionManagerCompressorFactory compressorStreamFactory, CompressAlgorithm algorithm)
    {
        _logger = logger.ForContextShort<TarCompressionManager>();
        _fs = fs;
        _compressorStreamFactory = compressorStreamFactory;

        FileExtension = "tar." + algorithm switch
        {
            CompressAlgorithm.Gzip => "gz",
            CompressAlgorithm.Xz => "xz",
            CompressAlgorithm.BZip2 => "bz2",
            var unsupportedAlgorithm => throw new NotSupportedException($"Compression algorithm {unsupportedAlgorithm} not supported")
        };
    }
    
    public string FileExtension { get; }
    
    public void CheckAvailable()
    {
        Marshal.PrelinkAll(typeof(BlockCompressorStream));
    }

    public void Compress(string archive,
        IEnumerable<string> files,
        string? changeDir = null,
        bool followSymlinks = false,
        CancellationToken cancellationToken = default)
    {
        using var archiveFile = _fs.File.Open(archive, FileMode.Create, FileAccess.Write);
        using var archiverStream = _compressorStreamFactory.Create(archiveFile);// new BlockCompressorStream(_compressor, archiveFile, BlockCompressorStreamBufferSize);
        using var tarArchiver = new TarArchiver(_fs, archiverStream, changeDir, followSymlinks);
        
        tarArchiver.OnEntryWritten += name => _logger.Information(name);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                tarArchiver.AddEntry(file);
            }
            catch (Exception e)
            {
                throw new IOException($"Unable to compress {file}", e);
            }
        }
    }
}