using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using TcfBackup.Action;
using TcfBackup.Database;
using TcfBackup.Factory;
using TcfBackup.Source;
using TcfBackup.Target;

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

    private static async Task WriteBackupInfoToDatabase(ISource source, ITarget target, CancellationToken cancellationToken)
    {
        var directory = target switch
        {
            DirTarget dirTarget => $"file://{dirTarget.Directory}",
            GDriveTarget gDriveTarget => $"gdrive://{gDriveTarget.Directory}",
            _ => throw new NotSupportedException(target.GetType().Name)
        };

        await using var db = new BackupDb();
        await using var trans = await db.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var file in source.GetFiles())
        {
            await db.InsertAsync(new Backup
            {
                Date = now,
                Path = Path.Join(directory, Path.GetFileName(file.Path))
            }, token: cancellationToken);
        }

        await trans.CommitAsync(cancellationToken);
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
                
                WriteBackupInfoToDatabase(result, target, cancellationToken)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
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