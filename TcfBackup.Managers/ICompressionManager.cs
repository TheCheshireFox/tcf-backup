namespace TcfBackup.Managers;

public interface ICompressionManager : IManager
{
    CompressAlgorithm CompressAlgorithm { get; }
    
    void Compress(Stream archive,
        IEnumerable<string> files,
        CancellationToken cancellationToken = default);
}