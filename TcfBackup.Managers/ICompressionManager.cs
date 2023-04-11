namespace TcfBackup.Managers;

public interface ICompressionManager
{
    CompressAlgorithm CompressAlgorithm { get; }
    
    void Compress(Stream archive,
        IEnumerable<string> files,
        CancellationToken cancellationToken = default);
}