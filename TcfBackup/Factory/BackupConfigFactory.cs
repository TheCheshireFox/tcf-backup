using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using TcfBackup.Action;
using TcfBackup.Configuration.Action;
using TcfBackup.Configuration.Source;
using TcfBackup.Configuration.Target;
using TcfBackup.Extensions.Configuration;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Source;
using TcfBackup.Target;

[module: UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Already handled by Microsoft.Extensions.Configuration")]

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
        { TargetType.Dir, typeof(DirectoryTargetOptions) },
        { TargetType.GDrive, typeof(GDriveTargetOptions) },
    };

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly IFileSystem _fs;
    private readonly IGDriveAdapter _gDriveAdapter;

    private IEncryptionManager CreateGpgEncryptionManager(GpgEncryptionActionOptions opts)
    {
        if (string.IsNullOrEmpty(opts.KeyFile) && string.IsNullOrEmpty(opts.KeyId))
        {
            throw new FormatException("No keyfile or signature specified for encryption manager.");
        }

        if (!string.IsNullOrEmpty(opts.KeyFile) && !string.IsNullOrEmpty(opts.KeyId))
        {
            throw new FormatException("Both the keyfile and signature are specified. You can choose only one.");
        }

        if (!string.IsNullOrEmpty(opts.KeyFile))
        {
            return GpgEncryptionManager.CreateWithKeyFile(_logger, _fs, opts.KeyFile, opts.Password);
        }

        if (!string.IsNullOrEmpty(opts.KeyId))
        {
            return GpgEncryptionManager.CreateWithKeyId(_logger, _fs, opts.KeyId, opts.Password);
        }

        throw new NotSupportedException("Specified gpg configuration not supported");
    }

    private IBtrfsManager CreateBtrfsManager()
    {
        return new BtrfsManager();
    }

    private IEncryptionManager CreateEncryptionManager(EncryptionActionOptions opts)
    {
        return opts switch
        {
            GpgEncryptionActionOptions gpgOpts => CreateGpgEncryptionManager(gpgOpts),
            _ => throw new ArgumentOutOfRangeException(nameof(opts), opts, null)
        };
    }

    private ILxdManager CreateLxdManager(LxdSourceOptions opts)
    {
        return new LxdManager(_logger, opts.Address);
    }

    private ICompressionManager CreateTarCompressionManager(IConfiguration configurationSection)
    {
        var compressorType = configurationSection.GetValue<TarCompressor>("compressor");
        var tarCompressionAlgorithm = compressorType switch
        {
            TarCompressor.Gzip => CompressAlgorithm.Gzip,
            TarCompressor.Xz => CompressAlgorithm.Xz,
            TarCompressor.BZip2 => CompressAlgorithm.BZip2,
            _ => throw new NotSupportedException($"Compression algorithm {compressorType} not supported")
        };

        var tarOptions = configurationSection.Get<TarOptions>() ?? new TarOptions();
        
        var optionsSection = configurationSection.GetSection("options");
        var factory = tarCompressionAlgorithm switch
        {
            CompressAlgorithm.Gzip => TarArchiverFactory.CreateGZip2(tarOptions, optionsSection.Get<GZipOptions?>()),
            CompressAlgorithm.Xz => TarArchiverFactory.CreateXz(tarOptions, optionsSection.Get<XzOptions?>()),
            CompressAlgorithm.BZip2 => TarArchiverFactory.CreateBZip(tarOptions, optionsSection.Get<BZip2Options?>()),
            _ => throw new NotSupportedException($"Compression algorithm {tarCompressionAlgorithm} not supported")
        };

        return new CompressionManager(_logger, factory, tarCompressionAlgorithm);
    }
    
    private IAction CreateCompressAction(CompressActionOptions opts, IConfiguration configurationSection)
    {
        var manager = opts.Engine switch
        {
            CompressEngine.Tar => CreateTarCompressionManager(configurationSection),
            var notSupportedEngine => throw new NotSupportedException($"Compression engine {notSupportedEngine} not supported")
        };

        return new CompressAction(_logger, manager, opts.Name);
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

        return new FilterAction(_logger, _fs, opts.Include ?? Array.Empty<string>(), opts.Exclude, opts.FollowSymlinks);
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
        IFileSystem fs,
        IGDriveAdapter gDriveAdapter,
        IConfiguration config)
    {
        _config = config;
        _logger = logger;
        _fs = fs;
        _gDriveAdapter = gDriveAdapter;

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
            BtrfsSourceOptions btrfsOpts => new BtrfsSource(_logger, CreateBtrfsManager(), _fs, btrfsOpts.Subvolume,
                btrfsOpts.SnapshotDir),
            DirectorySourceOptions dirOpts => new DirSource(_logger, _fs, dirOpts.Path),
            LxdSourceOptions lxdOpts => new LxdSnapshotSource(_logger, CreateLxdManager(lxdOpts), _fs, lxdOpts.Containers,
                lxdOpts.IgnoreMissing),
            var notSupportedSource => throw new NotSupportedException($"Source {notSupportedSource.Type} not supported")
        };
    }

    public IEnumerable<IAction> GetActions()
    {
        return _config.GetSection("actions").GetChildren().Select(s => (ActionOptions)s.Get(GetActionOptionsType) switch
        {
            CompressActionOptions compressOpts => CreateCompressAction(compressOpts, s),
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
            DirectoryTargetOptions dirOpts => new DirTarget(_logger, _fs, dirOpts.Path, dirOpts.Overwrite),
            GDriveTargetOptions gDriveOpts => new GDriveTarget(_logger, _gDriveAdapter, _fs, gDriveOpts.Path),
            var notSupportedTarget => throw new NotSupportedException($"Target {notSupportedTarget.Type} not supported")
        };
    }
}