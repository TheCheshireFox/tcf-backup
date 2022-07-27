using System.Linq;
using System.Threading;
using TcfBackup.Action;
using TcfBackup.Factory;
using TcfBackup.Source;

namespace TcfBackup;

public class BackupManager
{
    private readonly IFactory _factory;

    private static ISource ApplyAction(ISource source, IAction action, CancellationToken cancellationToken)
    {
        source.Prepare();
        try
        {
            return action.Apply(source, cancellationToken);
        }
        finally
        {
            source.Cleanup();
        }
    }

    public BackupManager(IFactory factory)
    {
        _factory = factory;
    }

    public void Backup(CancellationToken cancellationToken)
    {
        var source = _factory.GetSource();
        var target = _factory.GetTarget();
        var actions = _factory.GetActions().ToArray();

        source.Prepare();
        try
        {
            var result = actions.Length > 0
                ? actions
                    .Skip(1)
                    .Aggregate(actions[0].Apply(source, cancellationToken),
                        (src, action) => ApplyAction(src, action, cancellationToken))
                : source;
            try
            {
                target.Apply(result, cancellationToken);
            }
            finally
            {
                result.Cleanup();
            }
        }
        finally
        {
            source.Cleanup();
        }
    }
}