using System;
using System.Collections.Generic;
using System.Threading;
using TcfBackup.Filesystem;
using TcfBackup.Target;

namespace TcfBackup.Retention.BackupCleaners;

public class BackupCleanerFactory : IBackupCleanerFactory
{
    private readonly Dictionary<string, Lazy<IBackupCleaner>> _cleanersByScheme;

    public BackupCleanerFactory(IFileSystem fs, IGDriveAdapter gDriveAdapter)
    {
        _cleanersByScheme = new Dictionary<string, Lazy<IBackupCleaner>>
        {
            { TargetSchemes.Filesystem, new Lazy<IBackupCleaner>(() => new FilesystemBackupCleaner(fs), LazyThreadSafetyMode.ExecutionAndPublication) },
            { TargetSchemes.GDrive, new Lazy<IBackupCleaner>(() => new GDriveBackupCleaner(gDriveAdapter), LazyThreadSafetyMode.ExecutionAndPublication) }
        };
    }

    public IBackupCleaner GetByScheme(string scheme) =>
        _cleanersByScheme.TryGetValue(scheme, out var backupCleaner)
            ? backupCleaner.Value
            : throw new NotSupportedException(scheme);
}