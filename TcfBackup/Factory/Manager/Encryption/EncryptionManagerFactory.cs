using System;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;

namespace TcfBackup.Factory.Manager.Encryption;

public class EncryptionManagerFactoryScoped : ManagerFactoryScoped<IEncryptionManager, EncryptionManagerOptions>
{
    private readonly IFilesystem _fs;
    private readonly ILogger _logger;

    private IEncryptionManager CreateGpgEncryptionManager(EncryptionManagerOptions opts)
    {
        if (string.IsNullOrEmpty(opts.KeyFile) && string.IsNullOrEmpty(opts.Signature))
        {
            throw new FormatException("No keyfile or signature specified for encryption manager.");
        }

        if (!string.IsNullOrEmpty(opts.KeyFile) && !string.IsNullOrEmpty(opts.Signature))
        {
            throw new FormatException("Both the keyfile and signature are specified. You can choose only one.");
        }

        if (!string.IsNullOrEmpty(opts.KeyFile))
        {
            return GpgEncryptionManager.CreateWithKeyFile(_logger, _fs, opts.KeyFile, opts.Password);
        }

        if (!string.IsNullOrEmpty(opts.Signature))
        {
            return GpgEncryptionManager.CreateWithSignature(_logger, _fs, opts.Signature, opts.Password);
        }

        throw new NotSupportedException("Specified gpg configuration not supported");
    }
    
    public EncryptionManagerFactoryScoped(IFilesystem fs, ILogger logger)
    {
        _fs = fs;
        _logger = logger;
    }
    
    public IEncryptionManager Create(EncryptionManagerOptions selector) => selector switch
    {
        { Type: EncryptionManagerType.GpgLib } => CreateGpgEncryptionManager(selector),
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector.Type, null)
    };
}