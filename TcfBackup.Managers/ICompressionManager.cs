namespace TcfBackup.Managers;

public enum CompressAlgorithm
{
    BZip2,
    Xz,
    LZip,
    Lzma,
    Lzop,
    ZStd,
    Gzip
}

public interface ICompressionManager
{
    void Compress(CompressAlgorithm algorithm, string archive, string[] files, string? changeDir = null, bool followSymlinks = false, CancellationToken cancellationToken = default);
    IEnumerable<string> Decompress(string archive, string destination, CancellationToken cancellationToken = default);
}