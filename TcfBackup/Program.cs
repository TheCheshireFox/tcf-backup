using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TcfBackup.CommandLine.Options;
using TcfBackup.Commands;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using RetentionOptions = TcfBackup.CommandLine.Options.RetentionOptions;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TcfBackup;

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

    private static void InvokeCommands<TOption, TCommand>(TOption opts, IEnumerable<TCommand> commands)
        where TOption: GenericOptions
        where TCommand: ICommand<TOption>
    {
        WaitDebugger(opts);

        try
        {
            foreach (var command in commands)
            {
                using var serviceProvider = command.CreateServiceCollection(opts).BuildServiceProvider();

                AppEnvironment.Initialize(serviceProvider.GetRequiredService<IFileSystem>());
        
                var interruptionHandler = new InterruptionHandler();
                command.Invoke(opts, serviceProvider, interruptionHandler.Token);
            }
        }
        catch (Exception e)
        {
            OnException(opts, e);
        }
    }
    
    private static void ParsedRetention(RetentionOptions opts)
    {
        InvokeCommands(opts, opts.ConfigurationFiles.Select(configFile => new RetentionCommand(configFile)));
    }

    private static void ParsedBackup(BackupOptions opts)
    {
        InvokeCommands(opts, opts.ConfigurationFiles.Select(configFile => new BackupCommand(configFile)));
    }

    private static void ParsedGoogleAuth(GoogleAuthOptions opts)
    {
        InvokeCommands(opts, new []{ new GoogleAuthCommand() });
    }

    public static void Main(string[] args)
    {
        Parser.Default
            .ParseArguments<RetentionOptions, BackupOptions, GoogleAuthOptions>(args)
            .WithParsed<RetentionOptions>(ParsedRetention)
            .WithParsed<BackupOptions>(ParsedBackup)
            .WithParsed<GoogleAuthOptions>(ParsedGoogleAuth);
    }
}