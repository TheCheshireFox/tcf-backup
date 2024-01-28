using System.Diagnostics.CodeAnalysis;
using Serilog;
using TcfBackup.Action;
using TcfBackup.Configuration.Action;
using TcfBackup.Configuration.Action.CompressAction;
using TcfBackup.Configuration.Action.EncryptAction;
using TcfBackup.Configuration.Source;
using TcfBackup.Configuration.Target;
using TcfBackup.Factory.CompressionManager;
using TcfBackup.Filesystem;
using TcfBackup.Source;
using TcfBackup.Target;
using IConfigurationProvider = TcfBackup.Configuration.IConfigurationProvider;

[module: UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Already handled by Microsoft.Extensions.Configuration")]

namespace TcfBackup.Factory;

public class BackupConfigFactory : IFactory
{
    private readonly IConfigurationProvider _configurationProvider;
    private readonly ILogger _logger;
    private readonly IFileSystem _fs;
    private readonly IGDriveAdapter _gDriveAdapter;
    private readonly IEncryptionManagerFactory _encryptionManagerFactory;
    private readonly ISshManagerFactory _sshManagerFactory;
    private readonly IBtrfsManagerFactory _btrfsManagerFactory;
    private readonly ILxdManagerFactory _lxdManagerFactory;
    private readonly ICompressionManagerFactory _compressionManagerFactory;

    private IAction CreateCompressAction(CompressActionOptions opts)
    {
        return new CompressAction(_logger, _compressionManagerFactory.Create(opts), opts.Name);
    }

    private IAction CreateEncryptAction(EncryptActionOptions opts)
    {
        return new EncryptAction(_logger, _fs, _encryptionManagerFactory.Create(opts));
    }

    private IAction CreteFilterAction(FilterActionOptions opts)
    {
        if (opts.Include == null && opts.Exclude == null)
        {
            throw new FormatException("Filter action should have at least include or exclude regex string");
        }

        return new FilterAction(_logger, _fs, opts.Include ?? Array.Empty<string>(), opts.Exclude, opts.FollowSymlinks);
    }

    private IAction CreateRenameAction(RenameActionOptions opts)
    {
        if (string.IsNullOrEmpty(opts.Template))
        {
            throw new FormatException("Template cannot be empty");
        }

        return new RenameAction(_logger, _fs, opts.Template, opts.Overwrite);
    }

    public BackupConfigFactory(ILogger logger,
        IFileSystem fs,
        IGDriveAdapter gDriveAdapter,
        IConfigurationProvider configurationProvider,
        IEncryptionManagerFactory encryptionManagerFactory,
        ISshManagerFactory sshManagerFactory,
        IBtrfsManagerFactory btrfsManagerFactory,
        ILxdManagerFactory lxdManagerFactory,
        ICompressionManagerFactory compressionManagerFactory)
    {
        _configurationProvider = configurationProvider;
        _encryptionManagerFactory = encryptionManagerFactory;
        _sshManagerFactory = sshManagerFactory;
        _btrfsManagerFactory = btrfsManagerFactory;
        _lxdManagerFactory = lxdManagerFactory;
        _compressionManagerFactory = compressionManagerFactory;
        _logger = logger;
        _fs = fs;
        _gDriveAdapter = gDriveAdapter;
    }

    public ISource GetSource()
    {
        return _configurationProvider.GetSource() switch
        {
            BtrfsSourceOptions btrfsOpts => new BtrfsSource(_logger, _btrfsManagerFactory.Create(), _fs, btrfsOpts.Subvolume, btrfsOpts.SnapshotDir),
            DirectorySourceOptions dirOpts => new DirSource(_logger, _fs, dirOpts.Path),
            LxdSourceOptions lxdOpts => new LxdSnapshotSource(_logger, _lxdManagerFactory.Create(lxdOpts), _fs, lxdOpts.Containers, lxdOpts.IgnoreMissing),
            var unknown => throw new NotSupportedException($"Source {unknown.Type} not supported")
        };
    }

    public IEnumerable<IAction> GetActions()
    {
        return _configurationProvider.GetActions().Select(opts => opts switch
        {
            CompressActionOptions compressOpts => CreateCompressAction(compressOpts),
            EncryptActionOptions encryptOpts => CreateEncryptAction(encryptOpts),
            FilterActionOptions filterOpts => CreteFilterAction(filterOpts),
            RenameActionOptions renameOpts => CreateRenameAction(renameOpts),
            _ => throw new NotSupportedException($"Action {opts.Type} not supported")
        });
    }

    public ITarget GetTarget()
    {
        return _configurationProvider.GetTarget() switch
        {
            DirectoryTargetOptions dirOpts => new DirTarget(_logger, _fs, dirOpts.Path, dirOpts.Overwrite),
            GDriveTargetOptions gDriveOpts => new GDriveTarget(_logger, _gDriveAdapter, _fs, gDriveOpts.Path),
            SshTargetOptions sshTargetOpts => new SshTarget(_logger, _fs, _sshManagerFactory.Create(sshTargetOpts), sshTargetOpts.Path, sshTargetOpts.Overwrite),
            var unknown => throw new NotSupportedException($"Target {unknown.Type} not supported")
        };
    }
}