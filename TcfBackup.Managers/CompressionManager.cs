using System.Runtime.InteropServices;
using Serilog;
using TcfBackup.Archiver;
using TcfBackup.Archiver.Archivers;
using TcfBackup.Compressor;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Managers;

public class CompressionManager : ICompressionManager
{
    private class MultiDispose<T> : IDisposable
        where T: IDisposable
    {
        private readonly Stack<T> _disposables = new ();

        public T Add(T obj)
        {
            _disposables.Push(obj);
            return obj;
        }

        public void Dispose()
        {
            while (_disposables.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }
    }
    
    private readonly ILogger _logger;
    private readonly IFilesystem _fs;

    private void CompressWithTar(CompressAlgorithm[] algorithm, string archive, IEnumerable<string> files, string? changeDir, bool followSymlinks, CancellationToken cancellationToken)
    {
        using var archiveFile = _fs.Open(archive, FileMode.Create, FileAccess.Write);
        using var archivers = new MultiDispose<IStreamingArchiver>();
        
        Stream output = archiveFile;
        foreach (var algo in algorithm)
        {
            var archiver = archivers.Add(algo switch
            {
                CompressAlgorithm.Gzip => new GZipArchiver(output),
                CompressAlgorithm.Xz => new XzArchiver(output),
                CompressAlgorithm.BZip2 => new BZip2Archiver(output),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
            });
            output = archiver.Output;
        }
        
        using var tarArchiver = new TarArchiver(_fs, output, changeDir, followSymlinks);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            tarArchiver.AddEntry(file);
            _logger.Information(file);
        }
    }

    public CompressionManager(ILogger logger, IFilesystem fs)
    {
        _logger = logger.ForContextShort<CompressionManager>();
        _fs = fs;
    }
    
    public void CheckAvailable()
    {
        Marshal.PrelinkAll(typeof(CompressorNative));
    }

    public void Compress(CompressAlgorithm[] algorithm, string archive, IEnumerable<string> files, string? changeDir = null, bool followSymlinks = false, CancellationToken cancellationToken = default)
    {
        if (algorithm.Length == 0)
        {
            throw new ArgumentException("Compression method not selected", nameof(algorithm));
        }
        CompressWithTar(algorithm, archive, files, changeDir, followSymlinks, cancellationToken);
    }

    public IEnumerable<string> Decompress(string archive, string destination, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}