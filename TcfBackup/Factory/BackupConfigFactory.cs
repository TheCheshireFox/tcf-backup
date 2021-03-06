using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using TcfBackup.Action;
using TcfBackup.Configuration.Action;
using TcfBackup.Configuration.Source;
using TcfBackup.Configuration.Target;
using TcfBackup.Factory.Manager.Btrfs;
using TcfBackup.Factory.Manager.Compression;
using TcfBackup.Factory.Manager.Encryption;
using TcfBackup.Factory.Manager.Lxd;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Source;
using TcfBackup.Target;
using CompressAlgorithm = TcfBackup.Managers.CompressAlgorithm;

namespace TcfBackup.Factory;

public class BackupConfigFactory : IFactory
{
    private static readonly Dictionary<SourceType, Type> s_sourceTypeMapping = new()
    {
        { SourceType.Btrfs, typeof(BtrfsSourceOptions) },
        { SourceType.Directory, typeof(DirectorySourceOptions) },
        { SourceType.Lxd, typeof(LxdSourceOptions) },
    };

    private static readonly Dictionary<TargetType, Type> s_targetTypeMapping = new()
    {
        { TargetType.Directory, typeof(DirectoryTargetOptions) },
        { TargetType.GDrive, typeof(GDriveTargetOptions) },
    };

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly IFilesystem _fs;
    private readonly IGDriveAdapter _gDriveAdapter;

    private readonly BtrfsManagerFactoryScoped _btrfsManagerFactoryScoped;
    private readonly LxdManagerFactoryScoped _lxdManagerFactoryScoped;
    private readonly CompressionManagerFactoryScoped _compressionManagerFactoryScoped;
    private readonly EncryptionManagerFactoryScoped _encryptionManagerFactoryScoped;

    private IBtrfsManager CreateBtrfsManager()
    {
        return _btrfsManagerFactoryScoped.Create(BtrfsManagerType.Executable);
    }

    private ICompressionManager CreateCompressionManager()
    {
        return _compressionManagerFactoryScoped.Create(CompressionManagerType.TarExecutable);
    }

    private IEncryptionManager CreateEncryptionManager(EncryptionActionOptions opts)
    {
        return opts switch
        {
            GpgEncryptionActionOptions gpgOpts => _encryptionManagerFactoryScoped.Create(new EncryptionManagerOptions
            {
                Type = EncryptionManagerType.GpgLib,
                Password = opts.Password,
                KeyFile = opts.KeyFile,
                Signature = gpgOpts.Signature
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(opts), opts, null)
        };
    }

    private ILxdManager CreateLxdManager()
    {
        return _lxdManagerFactoryScoped.Create(LxdManagerType.Executable);
    }
    
    private IAction CreateCompressAction(CompressActionOptions opts)
    {
        var algo = opts.Algorithm switch
        {
            Configuration.Action.CompressAlgorithm.Gzip => CompressAlgorithm.Gzip,
            Configuration.Action.CompressAlgorithm.Lzma => CompressAlgorithm.Lzma,
            Configuration.Action.CompressAlgorithm.Lzop => CompressAlgorithm.Lzop,
            Configuration.Action.CompressAlgorithm.Xz => CompressAlgorithm.Xz,
            Configuration.Action.CompressAlgorithm.BZip2 => CompressAlgorithm.BZip2,
            Configuration.Action.CompressAlgorithm.LZip => CompressAlgorithm.LZip,
            Configuration.Action.CompressAlgorithm.ZStd => CompressAlgorithm.ZStd,
            _ => throw new NotSupportedException($"Compression algorithm {opts.Algorithm} not supported")
        };

        return new CompressAction(_logger, CreateCompressionManager(), _fs, algo, opts.Name, opts.ChangeDir, opts.FollowSymlinks);
    }

    private IAction CreateEncryptAction(EncryptionActionOptions opts)
    {
        return new EncryptAction(_logger, _fs, CreateEncryptionManager(opts));
    }

    private IAction CreteFilterAction(FilterActionOptions opts)
    {
        if (opts.Include == null && opts.Exclude == null)
        {
            throw new FormatException("Filter action should have at least include or exclude regex string");
        }

        return new FilterAction(_logger, opts.Include ?? Array.Empty<string>(), opts.Exclude, opts.FollowSymlinks);
    }

    private IAction CreateRenameAction(RenameActionOptions opts)
    {
        if (string.IsNullOrEmpty(opts.Template))
        {
            throw new FormatException("Template can not be empty");
        }

        return new RenameAction(_logger, _fs, opts.Template, opts.Overwrite);
    }

    private static Type GetActionOptionsType(IConfiguration cfg)
    {
        return cfg.GetValue<ActionType>("type") switch
        {
            ActionType.Compress => typeof(CompressActionOptions),
            ActionType.Filter => typeof(FilterActionOptions),
            ActionType.Rename => typeof(RenameActionOptions),
            ActionType.Encrypt => cfg.GetValue<EncryptionEngine>("engine") switch
            {
                EncryptionEngine.Openssl => typeof(OpensslEncryptionActionOptions),
                EncryptionEngine.Gpg => typeof(GpgEncryptionActionOptions),
                _ => throw new NotSupportedException($"Encryption engine {cfg.GetValue<EncryptionEngine>("engine")} not supported")
            },
            var notSupportedAction => throw new NotSupportedException($"Action {notSupportedAction} not supported")
        };
    }

    public BackupConfigFactory(ILogger logger,
        IFilesystem fs,
        BtrfsManagerFactoryScoped btrfsManagerFactoryScoped,
        LxdManagerFactoryScoped lxdManagerFactoryScoped,
        CompressionManagerFactoryScoped compressionManagerFactoryScoped,
        EncryptionManagerFactoryScoped encryptionManagerFactoryScoped,
        IGDriveAdapter gDriveAdapter,
        IConfiguration config)
    {
        _config = config;
        _logger = logger;
        _fs = fs;
        _gDriveAdapter = gDriveAdapter;
        
        _btrfsManagerFactoryScoped = btrfsManagerFactoryScoped;
        _lxdManagerFactoryScoped = lxdManagerFactoryScoped;
        _compressionManagerFactoryScoped = compressionManagerFactoryScoped;
        _encryptionManagerFactoryScoped = encryptionManagerFactoryScoped;

        if (_config == null)
        {
            throw new FormatException("Invalid configuration");
        }

        if (!_config.ContainsKey("source")) throw new FormatException("Source not specified");
        if (!_config.ContainsKey("target")) throw new FormatException("Target not specified");
        if (!_config.ContainsKey("actions")) throw new FormatException("Actions not specified");
    }

    public ISource GetSource()
    {
        var source = (SourceOptions)_config.Get(cfg => s_sourceTypeMapping[cfg.GetValue<SourceType>("type")], "source");
        return source switch
        {
            BtrfsSourceOptions btrfsOpts => new BtrfsSource(_logger, CreateBtrfsManager(), _fs, btrfsOpts.Subvolume, btrfsOpts.SnapshotDir),
            DirectorySourceOptions dirOpts => new DirSource(_logger, _fs, dirOpts.Path),
            LxdSourceOptions lxdOpts => new LxdSnapshotSource(_logger, CreateLxdManager(), _fs, lxdOpts.Containers, lxdOpts.IgnoreMissing),
            var notSupportedSource => throw new NotSupportedException($"Source {notSupportedSource.Type} not supported")
        };
    }

    public IEnumerable<IAction> GetActions()
    {
        return _config.GetSection("actions").GetChildren().Select(s => (ActionOptions)s.Get(GetActionOptionsType) switch
        {
            CompressActionOptions compressOpts => CreateCompressAction(compressOpts),
            EncryptionActionOptions encryptOpts => CreateEncryptAction(encryptOpts),
            FilterActionOptions filterOpts => CreteFilterAction(filterOpts),
            RenameActionOptions renameOpts => CreateRenameAction(renameOpts),
            var notSupportedAction => throw new NotSupportedException($"Action {notSupportedAction.Type} not supported")
        });
    }

    public ITarget GetTarget()
    {
        var target = (TargetOptions)_config.Get(cfg => s_targetTypeMapping[cfg.GetValue<TargetType>("type")], "target");

        return target switch
        {
            DirectoryTargetOptions dirOpts => new DirTarget(_fs, dirOpts.Path, dirOpts.Overwrite),
            GDriveTargetOptions gDriveOpts => new GDriveTarget(_logger, _gDriveAdapter, _fs, gDriveOpts.Path),
            var notSupportedTarget => throw new NotSupportedException($"Target {notSupportedTarget.Type} not supported")
        };
    }
}