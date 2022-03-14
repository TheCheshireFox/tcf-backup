using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using TcfBackup.Configuration;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Native;
using TcfBackup.Shared;

namespace TcfBackup
{
    public class GenericOptions
    {
        [Option('v', "verbose", Required = false, SetName = "Logging")]
        public bool Verbose { get; set; } = false;

        [Option('d', "debug", Required = false, SetName = "Logging")]
        public bool Debug { get; set; } = false;

#if DEBUG
        [Option("wait-debugger", Hidden = true, Required = false)]
        public bool WaitDebugger { get; set; }
#endif
    }

    [Verb("backup", HelpText = "Perform backups")]
    public class BackupOptions : GenericOptions
    {
        [Value(0, MetaName = "path", HelpText = "Configuration file", Required = true)]
        public string? ConfigurationFile { get; set; }
    }

    [Verb("restore", HelpText = "Restore managed backups")]
    public class RestoreOptions : GenericOptions
    {
        [Value(0, MetaName = "path", HelpText = "Configuration file", Required = true)]
        public string? ConfigurationFile { get; set; }
    }

    [Verb("google-auth", HelpText = "Perform google authentication")]
    public class GoogleAuthOptions : GenericOptions
    {
    }

    public class LoggerOptions
    {
        public LogEventLevel LogLevel { get; set; }
        public string Format { get; set; } = "[{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

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

        private static void ParsedBackup(BackupOptions opts)
        {
            WaitDebugger(opts);

            var config = ConfigurationFactory.CreateBackupConfiguration(opts);

            var di = new ServiceCollection()
                .Configure<GlobalOptions>(config.GetSection("global"))
                .Configure<LoggerOptions>(loggerOpts => loggerOpts.Fill(opts))
                .AddTransientFromFactory<LoggerFactory, ILogger>()
                .AddSingletonFromFactory<FilesystemFactory, IFilesystem>()
                .AddSingleton<IBtrfsManager, BtrfsManager>()
                .AddSingleton<ILxdManager, LxdManager>()
                .AddSingleton<ICompressionManager, TarCompressionManager>()
                .AddSingleton<IGDriveAdapter, GDriveAdapter>()
                .AddSingleton<IConfiguration>(config)
                .AddSingleton<IFactory, BackupConfigFactory>();

            var dp = di.BuildServiceProvider();

            AppEnvironment.Initialize(dp.GetService<IFilesystem>()!);

            try
            {
                var manager = dp.CreateService<BackupManager>();
                manager.Backup();
            }
            catch (Exception e)
            {
                dp.GetService<ILogger>()!.Fatal("{exc}", e);
            }
        }

        private static void ParsedRestore(RestoreOptions opts)
        {
            WaitDebugger(opts);
        }

        private static void ParsedGoogleAuth(GoogleAuthOptions opts)
        {
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
                dp.GetService<ILogger>()!.Fatal("{exc}", e);
            }
        }

        public static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<BackupOptions, RestoreOptions>(args)
                .WithParsed<BackupOptions>(ParsedBackup)
                .WithParsed<RestoreOptions>(ParsedRestore)
                .WithParsed<GoogleAuthOptions>(ParsedGoogleAuth);
        }
    }
}