using TcfBackup.Source;

namespace TcfBackup.Target;

public interface ITarget
{
    string Scheme { get; }
    string Directory { get; }
    
    void Apply(ISource source, CancellationToken cancellationToken);
}