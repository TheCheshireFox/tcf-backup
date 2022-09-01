using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TcfBackup.Action;
using TcfBackup.Configuration.Global;
using TcfBackup.Database.Repository;
using TcfBackup.Factory;
using TcfBackup.Retention;
using TcfBackup.Shared;
using TcfBackup.Source;
using TcfBackup.Target;

namespace TcfBackup;

public class BackupManager
{
    private readonly GlobalOptions _globalOptions;
    private readonly IFactory _factory;
    private readonly IBackupRepository _backupRepository;
    private readonly IRetentionManager _retentionManager;

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

    private async Task WriteBackups(ITarget target, ISource result, CancellationToken cancellationToken)
    {
        var targetDirectory = target.GetTargetDirectory();

        var files = result.GetFiles().Select(f => new BackupFile
        {
            Path = Path.Combine(targetDirectory, PathUtils.AppendRoot(f.Path))
        });

        var backup = new Backup
        {
            Date = DateTime.UtcNow,
            Name = _globalOptions.Name,
            Files = files
        };

        await _backupRepository.AddBackupAsync(backup, cancellationToken);
    }
    
    public BackupManager(IOptions<GlobalOptions> globalOptions, IFactory factory, IRetentionManager retentionManager, IBackupRepository backupRepository)
    {
        _globalOptions = globalOptions.Value;
        _factory = factory;
        _retentionManager = retentionManager;
        _backupRepository = backupRepository;
    }

    public async Task BackupAsync(CancellationToken cancellationToken)
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

                await WriteBackups(target, result, cancellationToken);
                await _retentionManager.PerformCleanupAsync(cancellationToken);
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
    
    public void Backup(CancellationToken cancellationToken)
    {
        BackupAsync(cancellationToken)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }
}