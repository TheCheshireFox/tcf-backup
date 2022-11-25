namespace TcfBackup.Managers;

public interface ICompressionManager : IManager
{
    string FileExtension { get; }
    
    void Compress(string archive,
        IEnumerable<string> files,
        string? changeDir = null,
        bool followSymlinks = false,
        CancellationToken cancellationToken = default);
}