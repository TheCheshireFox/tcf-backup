using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TcfBackup.BackupDatabase;
using TcfBackup.CommandLine.Options;
using TcfBackup.Configuration;
using TcfBackup.Configuration.Global;
using TcfBackup.Extensions.Configuration;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Retention;
using TcfBackup.Retention.BackupCleaners;
using TcfBackup.Shared;

namespace TcfBackup.Commands;

public abstract class ConfigBasedCommandBase<T> : ICommand<T>
    where T: GenericOptions
{
    private readonly string _configurationFile;

    protected ConfigBasedCommandBase(string configurationFile)
    {
        _configurationFile = configurationFile;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Already handled inside configuration.Get")]
    private static GlobalOptions BindGlobalOptions(IConfiguration configuration, string name)
    {
        var opts = configuration.Get<GlobalOptions>() ?? new GlobalOptions();
        opts.Name = name;

        return opts;
    }
    
    public IServiceCollection CreateServiceCollection(GenericOptions opts)
    {
        var globalConfig = ConfigurationFactory.CreateOrDefaultConfiguration(AppEnvironment.GlobalConfiguration);
        var config = ConfigurationFactory.CreateConfiguration(_configurationFile);
        globalConfig = globalConfig.Merge(config.GetSection("global"));

        return new ServiceCollection()
            .Configure(globalConfig, cfg => BindGlobalOptions(cfg, Path.GetFileNameWithoutExtension(_configurationFile)))
            .Configure<LoggerOptions>(loggerOpts => loggerOpts.Fill(opts))
            .Configure<Configuration.Global.RetentionOptions>(globalConfig.GetSection(nameof(GlobalOptions.Retention), StringComparison.InvariantCultureIgnoreCase))
            .AddTransientFromFactory<LoggerFactory, ILogger>()
            .AddSingletonFromFactory<FilesystemFactory, IFileSystem>()
            .AddSingleton<IBtrfsManager, BtrfsManager>()
            .AddSingleton<ILxdManager, LxdManager>()
            .AddSingleton<ICompressionManager, CompressionManager>()
            .AddSingleton<IGDriveAdapter, GDriveAdapter>()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<IFactory, BackupConfigFactory>()
            .AddSingleton<IBackupRepository, BackupRepository>(sp => new BackupRepository(sp.GetRequiredService<IFileSystem>(), AppEnvironment.TcfDatabaseDirectory))
            .AddSingleton<IBackupCleanerFactory, BackupCleanerFactory>()
            .AddSingleton<IRetentionManager, RetentionManager>();
    }

    public abstract void Invoke(T opts, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}