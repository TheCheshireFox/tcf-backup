using TcfBackup.Configuration.Action;
using TcfBackup.Configuration.Source;
using TcfBackup.Configuration.Target;

namespace TcfBackup.Configuration;

public interface IConfigurationProvider
{
    SourceOptions GetSource();
    TargetOptions GetTarget();
    IEnumerable<ActionOptions> GetActions();
}