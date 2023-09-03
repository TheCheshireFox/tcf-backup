using TcfBackup.Source;

namespace TcfBackup.Target;

public interface ITarget
{
    string Scheme { get; }
    string Directory { get; }
    
    IEnumerable<string> Apply(IFileListSource source, CancellationToken cancellationToken);
    IEnumerable<string> Apply(IStreamSource source, CancellationToken cancellationToken);
}