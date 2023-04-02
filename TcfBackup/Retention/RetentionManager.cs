using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using TcfBackup.BackupDatabase;
using TcfBackup.Configuration.Global;
using TcfBackup.Retention.BackupCleaners;
using TcfBackup.Shared;

namespace TcfBackup.Retention;

public class RetentionManager : IRetentionManager
{
    private readonly ILogger _logger;
    private readonly RetentionSchedule? _retentionSchedule;
    private readonly GlobalOptions _globalOptions;
    private readonly IBackupCleanerFactory _backupCleanerFactory;
    private readonly IBackupRepository _backupRepository;

    private void DebugLogBackups(IReadOnlyDictionary<int, Backup> backups)
    {
        if (!_logger.IsEnabled(LogEventLevel.Debug))
        {
            return;
        }
        
        foreach (var backup in backups)
        {
            _logger.Debug("Backup {Id} {Name} {Date}:", backup.Key, backup.Value.Name, backup.Value.Date);
            foreach (var file in backup.Value.Files)
            {
                _logger.Debug("\t{Path}", file.Path);
            }
        }
    }

    private void DebugLogBackupsForRemove(IEnumerable<Backup> backups)
    {
        if (!_logger.IsEnabled(LogEventLevel.Debug))
        {
            return;
        }
        
        foreach (var backup in backups)
        {
            _logger.Debug("Backup {Name} {Date}:", backup.Name, backup.Date);
        }
    }

    private async Task RemoveInvalid(CancellationToken cancellationToken)
    {
        _logger.Information("Cleaning up database...");
        
        var backups = _backupRepository
            .GetBackups()
            .Where(b => b.Name == _globalOptions.Name)
            .ToList();

        foreach (var backup in backups)
        {
            var validFiles = 0;
            
            foreach (var file in backup.Files)
            {
                var backupCleaner = _backupCleanerFactory.GetByScheme(new Uri(file.Path).Scheme);
                if (await backupCleaner.ExistsAsync(UriUtils.WithoutScheme(file.Path), cancellationToken))
                {
                    validFiles++;
                    continue;
                }
                
                _logger.Warning("Backup {Id} {Name} {Date} file {Path} not found", backup.Id, backup.Name, backup.Date, file.Path);
            }

            if (validFiles > 0)
            {
                continue;
            }
            
            _logger.Warning("Backup {Id} {Name} {Date} hasn't files. Removing from database...", backup.Id, backup.Name, backup.Date);
            _backupRepository.DeleteBackup(backup);
        }
        
        _logger.Information("Database cleaned up");
    }
    
    public RetentionManager(ILogger logger,
        IOptions<GlobalOptions> globalOptions,
        IOptions<RetentionOptions> retentionOptions,
        IBackupCleanerFactory backupCleanerFactory,
        IBackupRepository backupRepository)
    {
        _logger = logger.ForContextShort<RetentionManager>();
        
        var retentionScheduleStr = retentionOptions.Value.Schedule;
        _retentionSchedule = retentionScheduleStr != null
            ? RetentionSchedule.Parse(retentionScheduleStr)
            : null;
        _globalOptions = globalOptions.Value;
        _backupCleanerFactory = backupCleanerFactory;
        _backupRepository = backupRepository;
    }

    public async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        if (_retentionSchedule == null)
        {
            _logger.Information("No retention configured. Skipping...");
            return;
        }

        await RemoveInvalid(cancellationToken);
        
        _logger.Information("Performing cleanup...");

        var backups = _backupRepository
            .GetBackups()
            .Where(b => b.Name == _globalOptions.Name)
            .ToDictionary(b => b.Id, b => b);

        DebugLogBackups(backups);

        var backupDates = backups.ToDictionary(kv => kv.Key, kv => kv.Value.Date);
        var keysForDelete = _retentionSchedule
            .FilterForRemoval(backupDates)
            .ToHashSet();

        var backupsForDelete = backups
            .Where(kv => keysForDelete.Contains(kv.Key))
            .Select(kv => kv.Value)
            .ToList();

        DebugLogBackupsForRemove(backupsForDelete);
        
        if (!backupsForDelete.Any())
        {
            _logger.Information("Nothing to delete");
            return;
        }
        
        foreach (var backup in backupsForDelete)
        {
            foreach (var backupFile in backup.Files)
            {
                try
                {
                    var backupCleaner = _backupCleanerFactory.GetByScheme(new Uri(backupFile.Path).Scheme);

                    _logger.Information("Deleting {Path} with {Cleaner}", backupFile.Path, backupCleaner.GetType().Name);

                    await backupCleaner.DeleteAsync(UriUtils.WithoutScheme(backupFile.Path), cancellationToken);

                    _logger.Information("Done");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to delete {Path}", backupFile.Path);
                }
            }

            _backupRepository.DeleteBackup(backup);
        }
        
        _logger.Information("Cleanup complete");
    }
}