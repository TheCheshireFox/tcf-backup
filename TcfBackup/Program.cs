using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using TcfBackup.BackupDatabase;
using TcfBackup.CmdlineOptions;
using TcfBackup.Compressor;
using TcfBackup.Configuration;
using TcfBackup.Configuration.Global;
using TcfBackup.Extensions.Configuration;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Retention;
using TcfBackup.Retention.BackupCleaners;
using TcfBackup.Shared;
using RetentionOptions = TcfBackup.CmdlineOptions.RetentionOptions;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TcfBackup;

public class LoggerOptions
{
    public LogEventLevel LogLevel { get; private set; } = LogEventLevel.Information;
    public string Format => "[{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    public void Fill(GenericOptions opts)
    {
        LogLevel = opts switch
        {
            { Debug: true } => LogEventLevel.Debug,
            { Verbose: true } => LogEventLevel.Verbose,
            _ => LogEventLevel.Information
        };
    }
}

[SuppressMessage("ReSharper", "ExceptionPassedAsTemplateArgumentProblem")]
public static class Program
{
    private static void WaitDebugger(GenericOptions opts)
    {
#if DEBUG
        if (opts.WaitDebugger)
        {
            Console.WriteLine("Waiting for debugger...");
            while (!Debugger.IsAttached) Thread.Sleep(1000);
        }
#endif
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Already handled inside configuration.Get")]
    private static GlobalOptions BindGlobalOptions(IConfiguration configuration, string name)
    {
        var opts = configuration.Get<GlobalOptions>();
        opts.Name = name;

        return opts;
    }

    private static void OnException(GenericOptions opts, Exception exception)
    {
        var loggerOpts = new LoggerOptions();
        loggerOpts.Fill(opts);

        var loggerFactory = new LoggerFactory(new OptionsWrapper<LoggerOptions>(loggerOpts));
        var logger = loggerFactory.Create();

        if (exception is OperationCanceledException)
        {
            logger.Information("Operation cancelled");
        }
        else
        {
            logger.Fatal("{Exception}", exception);
        }
    }

    private static IServiceCollection CreateServiceCollection(GenericOptions opts, string configurationFile)
    {
        var globalConfig = ConfigurationFactory.CreateConfiguration(AppEnvironment.GlobalConfiguration);
        var config = ConfigurationFactory.CreateConfiguration(configurationFile);
        globalConfig = globalConfig.Merge(config.GetSection("global"));

        return new ServiceCollection()
            .Configure(globalConfig, cfg => BindGlobalOptions(cfg, Path.GetFileNameWithoutExtension(configurationFile)))
            .Configure<LoggerOptions>(loggerOpts => loggerOpts.Fill(opts))
            .Configure<Configuration.Global.RetentionOptions>(globalConfig.GetSection(nameof(GlobalOptions.Retention), StringComparison.InvariantCultureIgnoreCase))
            .AddTransientFromFactory<LoggerFactory, ILogger>()
            .AddSingletonFromFactory<FilesystemFactory, IFileSystem>()
            .AddSingleton<IBtrfsManager, BtrfsManager>()
            .AddSingleton<ILxdManager, LxdManager>()
            .AddSingleton<ICompressionManager, TarCompressionManager>()
            .AddSingleton<IGDriveAdapter, GDriveAdapter>()
            .AddSingleton<ICompressorStreamFactory, CompressorStreamFactory>()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<IFactory, BackupConfigFactory>()
            .AddSingleton<IBackupRepository, BackupRepository>(sp => new BackupRepository(sp.GetRequiredService<IFileSystem>(), AppEnvironment.TcfDatabaseDirectory))
            .AddSingleton<IBackupCleanerFactory, BackupCleanerFactory>()
            .AddSingleton<IRetentionManager, RetentionManager>();
    }
    
    private static void PerformBackup(GenericOptions opts, string configurationFile)
    {
        var di = CreateServiceCollection(opts, configurationFile);
        using var dp = di.BuildServiceProvider();

        AppEnvironment.Initialize(dp.GetService<IFileSystem>()!);

        var manager = dp.CreateService<BackupManager>();
        var interruptionHandler = new InterruptionHandler();

        manager.Backup(interruptionHandler.Token);
    }

    private static void PerformRetention(GenericOptions opts, string configurationFile)
    {
        var di = CreateServiceCollection(opts, configurationFile);
        using var dp = di.BuildServiceProvider();

        AppEnvironment.Initialize(dp.GetService<IFileSystem>()!);

        var retentionManager = dp.GetService<IRetentionManager>()!;
        var interruptionHandler = new InterruptionHandler();

        retentionManager.PerformCleanupAsync(interruptionHandler.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private static void ParsedRetention(RetentionOptions opts)
    {
        WaitDebugger(opts);

        try
        {
            foreach (var configFile in opts.ConfigurationFiles)
            {
                PerformRetention(opts, configFile);
            }
        }
        catch (Exception e)
        {
            OnException(opts, e);
        }
    }

    private static void ParsedBackup(BackupOptions opts)
    {
        WaitDebugger(opts);

        try
        {
            foreach (var configFile in opts.ConfigurationFiles)
            {
                PerformBackup(opts, configFile);
            }
        }
        catch (Exception e)
        {
            OnException(opts, e);
        }
    }

    private static void ParsedRestore(RestoreOptions opts)
    {
        throw new NotSupportedException();
    }

    private static void ParsedGoogleAuth(GoogleAuthOptions opts)
    {
        WaitDebugger(opts);

        var di = new ServiceCollection()
            .Configure<LoggerOptions>(loggerOpts => loggerOpts.Fill(opts))
            .AddTransientFromFactory<LoggerFactory, ILogger>()
            .AddSingletonFromFactory<FilesystemFactory, IFileSystem>()
            .AddSingleton<GDriveAdapter>();

        var dp = di.BuildServiceProvider();

        AppEnvironment.Initialize(dp.GetService<IFileSystem>()!);

        try
        {
            dp.GetService<GDriveAdapter>()!.Authorize();
        }
        catch (Exception e)
        {
            dp.GetService<ILogger>()!.Fatal("{Exception}", e);
        }
    }

    public static void Main(string[] args)
    {
        Parser.Default
            .ParseArguments<RetentionOptions, BackupOptions, RestoreOptions, GoogleAuthOptions>(args)
            .WithParsed<RetentionOptions>(ParsedRetention)
            .WithParsed<BackupOptions>(ParsedBackup)
            .WithParsed<RestoreOptions>(ParsedRestore)
            .WithParsed<GoogleAuthOptions>(ParsedGoogleAuth);
    }
}