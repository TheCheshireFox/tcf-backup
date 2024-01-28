using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TcfBackup.CommandLine.Options;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Commands;

public class GoogleAuthCommand : ICommand<GoogleAuthOptions>
{
    public IServiceCollection CreateServiceCollection(GenericOptions opts)
    {
        return new ServiceCollection()
            .Configure<LoggerOptions>(loggerOpts => loggerOpts.Fill(opts))
            .AddTransientFromFactory<LoggerFactory, ILogger>()
            .AddSingletonFromFactory<FilesystemFactory, IFileSystem>()
            .AddSingleton<IGDriveAdapter, GDriveAdapter>();
    }

    public void Invoke(GoogleAuthOptions opts, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        serviceProvider.GetRequiredService<IGDriveAdapter>().Authorize();
    }
}