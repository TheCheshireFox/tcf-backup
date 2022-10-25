namespace TcfBackup.Managers;

public enum CompressAlgorithm
{
    BZip2,
    Xz,
    Gzip
}

public interface ICompressionManager : IManager
{
    void Compress(CompressAlgorithm[] algorithm, string archive, IEnumerable<string> files, string? changeDir = null, bool followSymlinks = false, CancellationToken cancellationToken = default);
    IEnumerable<string> Decompress(string archive, string destination, CancellationToken cancellationToken = default);
}