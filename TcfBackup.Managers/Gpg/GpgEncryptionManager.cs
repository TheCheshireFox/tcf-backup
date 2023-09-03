using Libgpgme;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Managers.Gpg;

public class GpgEncryptionManager : IEncryptionManager
{
    private class GpgContext : IDisposable
    {
        private readonly IFileSystem _fs;
        
        public Context Context { get; }
        public Key Key { get; }
        
        public GpgContext(IFileSystem fs, Context context, Key key)
        {
            _fs = fs;
            Context = context;
            Key = key;
        }
        
        public void Dispose()
        {
            var homedir = Context.EngineInfo.HomeDir;
            
            Context.Dispose();
            Key.Dispose();
            
            _fs.Directory.Delete(homedir);
        }
    }

    private readonly ILogger _logger;
    private readonly IFileSystem _fs;
    private readonly Func<Context, Key> _getKey;
    private readonly string? _password;

    private static Key KeyFromKeyFile(IFileSystem fs, Context context, string keyfile)
    {
        if (!fs.File.Exists(keyfile))
        {
            throw new FileNotFoundException($"File or key {keyfile} not found");
        }

        using var keyStream = fs.File.Open(keyfile, FileMode.Open, FileAccess.Read);
        using var keyFileData = new GpgmeStreamData(keyStream);

        var importResult = context.KeyStore.Import(keyFileData);
        if (importResult.Imports == null)
        {
            throw new IOException($"Unable to import key {keyfile}");
        }

        if (importResult.Imports.Result != 0)
        {
            throw new IOException($"Unable to import key {keyfile}: {importResult.Imports.Result}");
        }

        return context.KeyStore.GetKey(importResult.Imports.Fpr, false);
    }

    private static Key KeyFromStore(Context context, string keyId)
    {
        return context.KeyStore
            .GetKeyList("", false)
            .FirstOrDefault(k => k.Fingerprint == keyId) ?? throw new Libgpgme.KeyNotFoundException($"Key with id {keyId} not found.");
    }

    private GpgContext PrepareContext()
    {
        var context = new Context();

        if (context.Protocol != Protocol.OpenPGP)
        {
            context.SetEngineInfo(Protocol.OpenPGP, null, _fs.Directory.CreateTempDir());
        }

        if (!string.IsNullOrEmpty(_password))
        {
            // ReSharper disable once RedundantAssignment
            context.SetPassphraseFunction((Context _, PassphraseInfo _, ref char[] passphrase) =>
            {
                passphrase = _password.ToCharArray();
                return PassphraseResult.Success;
            });
        }

        context.Armor = false;
        
        return new GpgContext(_fs, context, _getKey(context));
    }

    public static GpgEncryptionManager CreateWithKeyFile(ILogger logger, IFileSystem fs, string keyFile, string? password = null)
        => new(logger, fs, ctx => KeyFromKeyFile(fs, ctx, keyFile), password);

    public static GpgEncryptionManager CreateWithKeyId(ILogger logger, IFileSystem fs, string keyId, string? password = null)
        => new(logger, fs, ctx => KeyFromStore(ctx, keyId), password);

    private GpgEncryptionManager(ILogger logger, IFileSystem fs, Func<Context, Key> getKey, string? password)
    {
        _logger = logger.ForContextShort<GpgEncryptionManager>();
        _fs = fs;
        _getKey = getKey;
        _password = password;

        try
        {
            Gpgme.CheckVersion();
        }
        catch (Exception)
        {
            throw new FileNotFoundException("Unable to locate libgpg library.");
        }
    }

    public void Encrypt(Stream src, Stream dst, CancellationToken cancellationToken)
    {
        using var gpgContext = PrepareContext();

        var srcStreamWrapper = new ReaderStreamWrapper(src);
        var dstStreamWrapper = new WriterStreamWrapper(dst);

        using var srcStream = new GpgmeStreamData(srcStreamWrapper);
        using var dstStream = new GpgmeStreamData(dstStreamWrapper);

        _logger.Information("Encryption of the stream...");
        try
        {
            gpgContext.Context.Encrypt(new[] { gpgContext.Key }, EncryptFlags.AlwaysTrust, srcStream, dstStream);
        }
        catch (Exception e)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exceptions = new[] { srcStream.LastCallbackException, dstStream.LastCallbackException }
                .Where(exc => exc != null)
                .ToArray();

            if (exceptions.Length == 0)
            {
                throw;
            }

            throw new AggregateException(exceptions.Prepend(e));
        }
    }

    public void Encrypt(string src, string dst, CancellationToken cancellationToken)
    {
        using var srcRawStream = _fs.File.Open(src, FileMode.Open, FileAccess.Read);
        using var dstRawStream = _fs.File.Open(dst, FileMode.Create, FileAccess.Write);

        _logger.Information("Encryption of file {Source} to {Target}...", src, dst);
        Encrypt(srcRawStream, dstRawStream, cancellationToken);
    }

    public void Decrypt(string src, string dst, CancellationToken cancellationToken)
    {
        using var gpgContext = PrepareContext();

        using var srcRawStream = _fs.File.Open(src, FileMode.Open, FileAccess.Read);
        using var srcStream = new GpgmeStreamData(srcRawStream);
        using var dstRawStream = _fs.File.Open(dst, FileMode.Create, FileAccess.Write);
        using var dstStream = new GpgmeStreamData(dstRawStream);

        _logger.Information("Decryption of file {Source} to {Target}...", src, dst);
        try
        {
            gpgContext.Context.Decrypt(srcStream, dstStream);
        }
        catch (Exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }
}