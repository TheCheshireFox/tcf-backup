using TcfBackup.Configuration.Target;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Target;
using IConfigurationProvider = TcfBackup.Configuration.IConfigurationProvider;

namespace TcfBackup.Retention.BackupCleaners;

public class BackupCleanerFactory : IBackupCleanerFactory
{
    private readonly IFileSystem _fs;
    private readonly IGDriveAdapter _gDriveAdapter;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly ISshManagerFactory _sshManagerFactory;
    private readonly Dictionary<string, Lazy<IBackupCleaner>> _cleanersByScheme;

    public BackupCleanerFactory(IFileSystem fs,
        IGDriveAdapter gDriveAdapter,
        IConfigurationProvider configurationProvider,
        ISshManagerFactory sshManagerFactory)
    {
        _fs = fs;
        _gDriveAdapter = gDriveAdapter;
        _configurationProvider = configurationProvider;
        _sshManagerFactory = sshManagerFactory;
        _cleanersByScheme = new Dictionary<string, Lazy<IBackupCleaner>>
        {
            { TargetSchemes.Filesystem, new Lazy<IBackupCleaner>(CreateFilesystemBackupCleaner, LazyThreadSafetyMode.ExecutionAndPublication) },
            { TargetSchemes.GDrive, new Lazy<IBackupCleaner>(CreateGDriveBackupCleaner, LazyThreadSafetyMode.ExecutionAndPublication) },
            { TargetSchemes.Ssh, new Lazy<IBackupCleaner>(CreateSshBackupCleaner, LazyThreadSafetyMode.ExecutionAndPublication) },
        };
    }

    public IBackupCleaner GetByScheme(string scheme) =>
        _cleanersByScheme.TryGetValue(scheme, out var backupCleaner)
            ? backupCleaner.Value
            : throw new NotSupportedException(scheme);

    private FilesystemBackupCleaner CreateFilesystemBackupCleaner() => new(_fs);

    private GDriveBackupCleaner CreateGDriveBackupCleaner() => new(_gDriveAdapter);
    
    private SshBackupCleaner CreateSshBackupCleaner()
    {
        if (_configurationProvider.GetTarget() is not SshTargetOptions sshTargetOpts)
        {
            throw new InvalidOperationException("Configuration doesn't contain ssh target");
        }

        return new SshBackupCleaner(_sshManagerFactory.Create(sshTargetOpts));
    }
}