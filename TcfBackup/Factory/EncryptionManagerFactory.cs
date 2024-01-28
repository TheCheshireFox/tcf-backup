using Serilog;
using TcfBackup.Configuration.Action.EncryptAction;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Managers.Gpg;

namespace TcfBackup.Factory;

public interface IEncryptionManagerFactory
{
    IEncryptionManager Create(EncryptActionOptions opts);
}

public class EncryptionManagerFactory : IEncryptionManagerFactory
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fs;

    public EncryptionManagerFactory(ILogger logger, IFileSystem fs)
    {
        _logger = logger;
        _fs = fs;
    }

    public IEncryptionManager Create(EncryptActionOptions opts)
    {
        return opts switch
        {
            GpgEncryptActionOptions gpgOpts => Create(gpgOpts),
            _ => throw new ArgumentOutOfRangeException(nameof(opts), opts, null)
        };
    }
    
    private GpgEncryptionManager Create(GpgEncryptActionOptions opts)
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
}