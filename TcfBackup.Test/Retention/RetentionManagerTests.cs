using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.BackupDatabase;
using TcfBackup.Configuration;
using TcfBackup.Configuration.Global;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Retention;
using TcfBackup.Retention.BackupCleaners;
using TcfBackup.Shared;

namespace TcfBackup.Test.Retention;

public class RetentionManagerTests
{
    private class BackupEqualityComparer : IEqualityComparer<Backup>
    {
        public bool Equals(Backup? x, Backup? y) => x != null && y != null && x.Id == y.Id;
        public int GetHashCode(Backup obj) => HashCode.Combine(obj.Id, obj.Name, obj.Date, obj.Files);
    }

    private static Backup MakeBackup(int id, DateTime date, string path, string name = "test") =>
        new ()
        {
            Id = id,
            Date = date,
            Name = name,
            Files = new List<BackupFile>(new [] { new BackupFile(path) })
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

    private IEnumerable<(IBackupCleaner, BackupFile)> GetBackupFiles(IBackupCleanerFactory backupCleanerFactory, IRetentionCleanupTestCase testCase)
    {
        return testCase.Backups
            .SelectMany(b => b.Files)
            .Select(f => (backupCleanerFactory.GetByScheme(new Uri(f.Path).Scheme), f));
    }
    
    private void PrepareCleanerDependencies(IBackupCleanerFactory backupCleanerFactory, Mock<IFileSystem> fs,
        Mock<IGDriveAdapter> gDriveAdapter, IRetentionCleanupTestCase testCase)
    {
        foreach (var (cleaner, backup) in GetBackupFiles(backupCleanerFactory, testCase))
        {
            var path = UriUtils.WithoutScheme(backup.Path);
            switch (cleaner)
            {
                case FilesystemBackupCleaner:
                    fs.Setup(f => f.File.Exists(path)).Returns(true);
                    fs.Setup(f => f.File.Delete(path));
                    break;
                case GDriveBackupCleaner:
                    gDriveAdapter.Setup(g => g.ExistsAsync(path, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
                    gDriveAdapter.Setup(g => g.DeleteFileAsync(path, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
                    break;
            }
        }
    }
    
    [Test]
    [TestCase(typeof(FilesystemRetentionCleanupTestCase))]
    [TestCase(typeof(GDriveRetentionCleanupTestCase))]
    [TestCase(typeof(OnlyMatchingNameRetentionCleanupTestCase))]
    // TODO: ssh tests
    public async Task RetentionCleanupTest(Type testScheduleType)
    {
        var testCase = (IRetentionCleanupTestCase)Activator.CreateInstance(testScheduleType)!;

        var fs = new Mock<IFileSystem>(MockBehavior.Strict);
        var configurationProvider = new Mock<IConfigurationProvider>(MockBehavior.Strict);
        var gDriveAdapter = new Mock<IGDriveAdapter>(MockBehavior.Strict);
        var sshManagerFactory = new Mock<ISshManagerFactory>();
        var backupCleanerFactory = new BackupCleanerFactory(fs.Object, gDriveAdapter.Object, configurationProvider.Object, sshManagerFactory.Object);

        PrepareCleanerDependencies(backupCleanerFactory, fs, gDriveAdapter, testCase);

        var backupRepository = new Mock<IBackupRepository>(MockBehavior.Strict);
        backupRepository.Setup(r => r.GetBackups()).Returns(testCase.Backups.AsEnumerable());
        backupRepository.Setup(r => r.DeleteBackup(It.IsIn(testCase.ExpectedToRemove, new BackupEqualityComparer())));

        var logger = new LoggerConfiguration().CreateLogger();
        var retentionManager = new RetentionManager(
            logger,
            Options.Create(testCase.GlobalOptions),
            Options.Create(testCase.RetentionOptions),
            backupCleanerFactory,
            backupRepository.Object);

        await retentionManager.PerformCleanupAsync(CancellationToken.None);
    }
}