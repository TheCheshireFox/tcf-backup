using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TcfBackup.CommandLine.Options;

namespace TcfBackup.Commands;

public class BackupCommand : ConfigBasedCommandBase<BackupOptions>
{
    public BackupCommand(string configurationFile) : base(configurationFile)
    {
    }

    public override void Invoke(BackupOptions opts, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var manager = ActivatorUtilities.CreateInstance<BackupManager>(serviceProvider);
        manager.Backup(cancellationToken);
    }
}