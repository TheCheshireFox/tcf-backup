using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using TcfBackup.Action;
using TcfBackup.BackupDatabase;
using TcfBackup.Configuration.Global;
using TcfBackup.Factory;
using TcfBackup.Retention;
using TcfBackup.Source;
using TcfBackup.Target;

namespace TcfBackup;

public class BackupActionContext : IActionContext, IDisposable
{
    private ISource _currentSource;
    private readonly List<ISource> _sources = new();

    public BackupActionContext(ISource source)
    {
        _currentSource = source;
    }
    
    public bool TryGetFileListSource(out IFileListSource source)
    {
        if (_currentSource is IFileListSource fileListSource)
        {
            source = fileListSource;
            return true;
        }

        source = default!;
        return false;
    }

    public bool TryGetStreamSource(out IStreamSource source)
    {
        if (_currentSource is IStreamSource streamSource)
        {
            source = streamSource;
            return true;
        }

        source = default!;
        return false;
    }

    public void SetResult(IFileListSource source)
    {
        _sources.Add(_currentSource = source);
    }

    public void SetResult(IStreamSource source)
    {
        _sources.Add(_currentSource = source);
    }

    public void Dispose()
    {
        foreach (var source in ((IEnumerable<ISource>)_sources).Reverse())
        {
            try
            {
                source.Cleanup(CancellationToken.None);
            }
            catch (Exception)
            {
                // NOP
            }
        }

        GC.SuppressFinalize(this);
    }
}

public class BackupManager
{
    private readonly GlobalOptions _globalOptions;
    private readonly ILogger _logger;
    private readonly IFactory _factory;
    private readonly IBackupRepository _backupRepository;
    private readonly IRetentionManager _retentionManager;

    private void LogBackup(Backup backup)
    {
        if (!_logger.IsEnabled(LogEventLevel.Debug))
        {
            return;
        }
        
        _logger.Debug("Writing backup record: {Name} {Date}", backup.Name, backup.Date);
        foreach (var file in backup.Files)
        {
            _logger.Debug("\t{Path}", file.Path);
        }
    }
    
    private void WriteBackups(ITarget target, IEnumerable<string> result)
    {
        var targetDirectory = target.GetTargetDirectory();
        
        var files = result.Select(f => new BackupFile
        {
            Path = Path.Join(targetDirectory, Path.GetFileName(f))
        });
        
        var backup = new Backup
        {
            Date = DateTime.UtcNow,
            Name = _globalOptions.Name,
            Files = files.ToList()
        };

        LogBackup(backup);
        
        _backupRepository.AddBackup(backup);
    }
    
    public BackupManager(IOptions<GlobalOptions> globalOptions, ILogger logger, IFactory factory, IRetentionManager retentionManager, IBackupRepository backupRepository)
    {
        _globalOptions = globalOptions.Value;
        _logger = logger;
        _factory = factory;
        _retentionManager = retentionManager;
        _backupRepository = backupRepository;
    }

    public async Task BackupAsync(CancellationToken cancellationToken)
    {
        var source = _factory.GetSource();
        var target = _factory.GetTarget();
        var actions = _factory.GetActions().ToArray();

        source.Prepare(cancellationToken);

        try
        {
            using var context = new BackupActionContext(source);
            
            foreach (var action in actions)
            {
                await action.ApplyAsync(context, cancellationToken);
            }

            IEnumerable<string> result;
            if (context.TryGetStreamSource(out var streamSource))
            {
                result = target.Apply(streamSource, cancellationToken);
            }
            else if (context.TryGetFileListSource(out var fileListSource))
            {
                result = target.Apply(fileListSource, cancellationToken);
            }
            else
            {
                throw new Exception("BUG: Unknown source type");
            }

            WriteBackups(target, result);
            await _retentionManager.PerformCleanupAsync(cancellationToken);
        }
        finally
        {
            source.Cleanup(cancellationToken);
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