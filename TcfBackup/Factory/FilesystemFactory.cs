using Microsoft.Extensions.Options;
using Serilog;
using TcfBackup.Configuration;
using TcfBackup.Configuration.Global;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Factory;

public class FilesystemFactory : IServiceCollectionFactory<IFilesystem>
{
    private readonly string? _workingDir;

    public FilesystemFactory(ILogger logger, IOptions<GlobalOptions> globalOptions)
    {
        _workingDir = globalOptions.Value?.WorkingDir;
        if (!string.IsNullOrEmpty(_workingDir))
        {
            logger.Information("Working directory: {workingDir}", _workingDir);
        }
    }

    public IFilesystem Create() => new Filesystem.Filesystem(_workingDir);
}