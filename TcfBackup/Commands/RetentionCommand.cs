using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TcfBackup.CommandLine.Options;
using TcfBackup.Retention;

namespace TcfBackup.Commands;

public class RetentionCommand : ConfigBasedCommandBase<RetentionOptions>
{
    public RetentionCommand(string configurationFile) : base(configurationFile)
    {
    }

    public override void Invoke(RetentionOptions opts, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var retentionManager = serviceProvider.GetRequiredService<IRetentionManager>();
        retentionManager.PerformCleanupAsync(cancellationToken).GetAwaiter().GetResult();
    }
}