using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;
using TcfBackup.Configuration.Global;
using TcfBackup.Database;
using TcfBackup.Database.Repository;
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

    public RetentionManager(ILogger logger,
        IOptions<GlobalOptions> globalOptions,
        IOptions<RetentionOptions> retentionOptions,
        IBackupCleanerFactory backupCleanerFactory,
        IBackupRepository backupRepository)
    {
        _logger = logger.ForContextShort<RetentionManager>();
        
        var retentionScheduleStr = retentionOptions.Value?.Schedule;
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
        
        _logger.Information("Performing cleanup...");
        
        var backups = (await _backupRepository
            .GetBackupsAsync())
            .Where(b => b.Name == _globalOptions.Name)
            .ToDictionary(b => b.Id, b => b);

        var backupDates = backups.ToDictionary(kv => kv.Key, kv => kv.Value.Date);
        var keysForDelete = _retentionSchedule
            .FilterForRemoval(backupDates)
            .ToHashSet();

        var backupsForDelete = backups
            .Where(kv => keysForDelete.Contains(kv.Key))
            .Select(kv => kv.Value)
            .ToList();

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
                
                    await backupCleaner.DeleteAsync(UriUtils.WithoutScheme(backupFile.Path));

                    _logger.Information("Done");
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to delete {Path}", backupFile.Path);
                }
            }

            await _backupRepository.DeleteBackupAsync(backup, cancellationToken);
        }
        
        _logger.Information("Cleanup complete");
    }
}