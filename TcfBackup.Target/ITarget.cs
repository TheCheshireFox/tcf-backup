using TcfBackup.Source;

namespace TcfBackup.Target;

public interface ITarget
{
    void Apply(ISource source, CancellationToken cancellationToken);
}