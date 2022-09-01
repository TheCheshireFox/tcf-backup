using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Configuration.Global;
using TcfBackup.Database.Repository;
using TcfBackup.Filesystem;
using TcfBackup.Retention;
using TcfBackup.Retention.BackupCleaners;
using TcfBackup.Shared;

namespace TcfBackup.Test.Retention;

public class RetentionManagerTests
{
    private static readonly IEqualityComparer<Backup> s_backupComparer = new FuncEqualityComparer<Backup>(
        (l, r) => l.BackupId == r.BackupId);

    private static Backup MakeBackup(int id, DateTime date, string path, string name = "test") =>
        new ()
        {
            Date = date,
            Name = name
        };
    
    private interface IRetentionCleanupTestCase
    {
        GlobalOptions GlobalOptions { get; }
        RetentionOptions RetentionOptions { get; }
        Backup[] Backups { get; }
        Backup[] ExpectedToRemove { get; }
    }

    private class FilesystemRetentionCleanupTestCase : IRetentionCleanupTestCase
    {
        private static readonly DateTime s_start = new (2020, 1, 1);

        public GlobalOptions GlobalOptions => new() { Name = "test" };
        public RetentionOptions RetentionOptions => new() { Schedule = "2d" };

        public Backup[] Backups => new[]
        {
            MakeBackup(0, s_start.AddDays(0), "file:///dev/f1"),
            MakeBackup(1, s_start.AddDays(1), "file:///dev/f2"),
            MakeBackup(2, s_start.AddDays(2), "file:///dev/f3"),
        };

        public Backup[] ExpectedToRemove => new[]
        {
            MakeBackup(0, s_start.AddDays(0), "file:///dev/f1")
        };
    }
    
    private class GDriveRetentionCleanupTestCase : IRetentionCleanupTestCase
    {
        private static readonly DateTime s_start = new (2020, 1, 1);
        
        public GlobalOptions GlobalOptions => new() { Name = "test" };
        public RetentionOptions RetentionOptions => new() { Schedule = "2d" };

        public Backup[] Backups => new[]
        {
            MakeBackup(0, s_start.AddDays(0), "gdrive:///dev/f1"),
            MakeBackup(1, s_start.AddDays(1), "gdrive:///dev/f2"),
            MakeBackup(2, s_start.AddDays(2), "gdrive:///dev/f3"),
        };

        public Backup[] ExpectedToRemove => new[]
        {
            MakeBackup(0, s_start.AddDays(0), "gdrive:///dev/f1")
        };
    }
    
    private class OnlyMatchingNameRetentionCleanupTestCase : IRetentionCleanupTestCase
    {
        private static readonly DateTime s_start = new (2020, 1, 1);
        
        public GlobalOptions GlobalOptions => new() { Name = "gdrive" };
        public RetentionOptions RetentionOptions => new() { Schedule = "2d" };

        public Backup[] Backups => new[]
        {
            MakeBackup(0, s_start.AddDays(0), "gdrive:///dev/f1", "gdrive"),
            MakeBackup(1, s_start.AddDays(1), "gdrive:///dev/f2", "gdrive"),
            MakeBackup(2, s_start.AddDays(2), "gdrive:///dev/f3", "gdrive"),
            MakeBackup(3, s_start.AddDays(0), "file:///dev/f1", "file"),
            MakeBackup(4, s_start.AddDays(1), "file:///dev/f2", "file"),
            MakeBackup(5, s_start.AddDays(2), "file:///dev/f3", "file"),
        };

        public Backup[] ExpectedToRemove => new[]
        {
            MakeBackup(0, s_start.AddDays(0), "gdrive:///dev/f1", "gdrive")
        };
    }

    private void PrepareCleanerDependencies(BackupCleanerFactory backupCleanerFactory, Mock<IFilesystem> fs,
        Mock<IGDriveAdapter> gDriveAdapter, IEnumerable<Backup> backupsToRemove)
    {
        var cleanersByBackups = backupsToRemove.SelectMany(b => b.Files)
            .ToDictionary(b => b, b => backupCleanerFactory.GetByScheme(new Uri(b.Path).Scheme));
        
        foreach (var (backup, cleaner) in cleanersByBackups)
        {
            var path = UriUtils.WithoutScheme(backup.Path);
            switch (cleaner)
            {
                case FilesystemBackupCleaner:
                    fs.Setup(f => f.Delete(path));
                    break;
                case GDriveBackupCleaner:
                    gDriveAdapter.Setup(g => g.DeleteFile(path));
                    break;
            }
        }
    }
    
    [Test]
    [TestCase(typeof(FilesystemRetentionCleanupTestCase))]
    [TestCase(typeof(GDriveRetentionCleanupTestCase))]
    [TestCase(typeof(OnlyMatchingNameRetentionCleanupTestCase))]
    public async Task RetentionCleanupTest(Type testScheduleType)
    {
        var testCase = (IRetentionCleanupTestCase)Activator.CreateInstance(testScheduleType)!;

        var fs = new Mock<IFilesystem>(MockBehavior.Strict);
        var gDriveAdapter = new Mock<IGDriveAdapter>(MockBehavior.Strict);
        var backupCleanerFactory = new BackupCleanerFactory(fs.Object, gDriveAdapter.Object);

        PrepareCleanerDependencies(backupCleanerFactory, fs, gDriveAdapter, testCase.ExpectedToRemove);

        var backupRepository = new Mock<IBackupRepository>(MockBehavior.Strict);
        backupRepository.Setup(r => r.GetBackupsAsync()).Returns(Task.FromResult(testCase.Backups.AsEnumerable()));
        backupRepository.Setup(r => r.DeleteBackupAsync(It.IsIn(testCase.ExpectedToRemove, s_backupComparer), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var logger = new LoggerConfiguration().CreateLogger();
        var retentionManager = new RetentionManager(
            logger,
            Options.Create(testCase.GlobalOptions),
            Options.Create(testCase.RetentionOptions),
            backupCleanerFactory,
            backupRepository.Object);

        await retentionManager.PerformCleanupAsync(CancellationToken.None);
        
        fs.VerifyAll();
        gDriveAdapter.VerifyAll();
        backupRepository.VerifyAll();
    }
}