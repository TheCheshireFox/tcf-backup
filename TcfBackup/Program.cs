using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using TcfBackup.CmdlineOptions;
using TcfBackup.Configuration;
using TcfBackup.Configuration.Global;
using TcfBackup.Extensions.Configuration;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TcfBackup;

public class LoggerOptions
{
    public LogEventLevel LogLevel { get; private set; }
    public string Format { get; } = "[{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

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

    private static GlobalOptions BindGlobalOptions(IConfiguration configuration)
    {
        var opts = configuration.Get<GlobalOptions>();

        if (opts.Database != null)
        {
            opts.Database = opts.Database.Type switch
            {
                DatabaseType.Local => configuration.GetSection("database").Get<LocalDatabase>(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return opts;
    }

    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    private static void PerformBackup(GenericOptions opts, string configurationFile)
    {
        var globalConfig = ConfigurationFactory.CreateConfiguration(AppEnvironment.GlobalConfiguration);
        var config = ConfigurationFactory.CreateConfiguration(configurationFile);
        globalConfig = globalConfig.Merge(config.GetSection("global"));

        var di = new ServiceCollection()
            .Configure(globalConfig, BindGlobalOptions)
            .Configure<LoggerOptions>(loggerOpts => loggerOpts.Fill(opts))
            .AddTransientFromFactory<LoggerFactory, ILogger>()
            .AddSingletonFromFactory<FilesystemFactory, IFilesystem>()
            .AddSingleton<IBtrfsManager, BtrfsManager>()
            .AddSingleton<ILxdManager, LxdManager>()
            .AddSingleton<ICompressionManager, TarCompressionManager>()
            .AddSingleton<IGDriveAdapter, GDriveAdapter>()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<IFactory, BackupConfigFactory>();

        using var dp = di.BuildServiceProvider();

        AppEnvironment.Initialize(dp.GetService<IFilesystem>()!);

        var manager = dp.CreateService<BackupManager>();
        var interruptionHandler = new InterruptionHandler();

        manager.Backup(interruptionHandler.Token);
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
            var loggerOpts = new LoggerOptions();
            loggerOpts.Fill(opts);

            var loggerFactory = new LoggerFactory(new OptionsWrapper<LoggerOptions>(loggerOpts));
            var logger = loggerFactory.Create();
            
            if (e is OperationCanceledException)
            {
                logger.Information("Operation cancelled");
            }
            else
            {
                logger.Fatal("{Exception}", e);
            }
        }
    }

    private static void ParsedRestore(RestoreOptions opts)
    {
        WaitDebugger(opts);
    }

    private static void ParsedGoogleAuth(GoogleAuthOptions opts)
    {
        WaitDebugger(opts);
        
        var di = new ServiceCollection()
            .Configure<LoggerOptions>(loggerOpts => loggerOpts.Fill(opts))
            .AddTransientFromFactory<LoggerFactory, ILogger>()
            .AddSingletonFromFactory<FilesystemFactory, IFilesystem>()
            .AddSingleton<GDriveAdapter>();

        var dp = di.BuildServiceProvider();

        AppEnvironment.Initialize(dp.GetService<IFilesystem>()!);

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
            .ParseArguments<BackupOptions, RestoreOptions, GoogleAuthOptions>(args)
            .WithParsed<BackupOptions>(ParsedBackup)
            .WithParsed<RestoreOptions>(ParsedRestore)
            .WithParsed<GoogleAuthOptions>(ParsedGoogleAuth);
    }
}