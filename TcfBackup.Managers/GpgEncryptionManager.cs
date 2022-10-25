using Libgpgme;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Managers;

public class GpgEncryptionManager : IEncryptionManager
{
    private record GpgContext(Context Context, Key Key) : IDisposable
    {
        public void Dispose()
        {
            Context.Dispose();
            Key.Dispose();
        }
    }

    private readonly ILogger _logger;
    private readonly IFilesystem _fs;
    private readonly Func<Context, Key> _getKey;
    private readonly string? _password;

    private static Key KeyFromKeyFile(IFilesystem fs, Context context, string keyfile)
    {
        if (!fs.FileExists(keyfile))
        {
            throw new FileNotFoundException($"File or key {keyfile} not found");
        }

        using var keyStream = fs.Open(keyfile, FileMode.Open, FileAccess.Read);
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

    private static Key KeyFromStore(Context context, string signature)
    {
        return context.KeyStore
            .GetKeyList("", false)
            .FirstOrDefault(k => k.Fingerprint == signature) ?? throw new Libgpgme.KeyNotFoundException($"Key with signature {signature} not found.");
    }

    private GpgContext PrepareContext()
    {
        var context = new Context();

        if (context.Protocol != Protocol.OpenPGP)
        {
            context.SetEngineInfo(Protocol.OpenPGP, null, null);
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

        return new GpgContext(context, _getKey(context));
    }

    public static GpgEncryptionManager CreateWithKeyFile(ILogger logger, IFilesystem fs, string keyFile, string? password = null)
        => new(logger, fs, ctx => KeyFromKeyFile(fs, ctx, keyFile), password);

    public static GpgEncryptionManager CreateWithSignature(ILogger logger, IFilesystem fs, string signature, string? password = null)
        => new(logger, fs, ctx => KeyFromStore(ctx, signature), password);

    private GpgEncryptionManager(ILogger logger, IFilesystem fs, Func<Context, Key> getKey, string? password)
    {
        _logger = logger.ForContextShort<GpgEncryptionManager>();
        _fs = fs;
        _getKey = getKey;
        _password = password;
    }

    public void CheckAvailable()
    {
        try
        {
            Gpgme.CheckVersion();
        }
        catch (Exception)
        {
            throw new FileNotFoundException("Unable to locate libgpg library.");
        }
    }
    
    public void Encrypt(string src, string dst, CancellationToken cancellationToken)
    {
        using var gpgContext = PrepareContext();

        using var srcRawStream = _fs.Open(src, FileMode.Open, FileAccess.Read);
        using var srcStream = new GpgmeStreamData(srcRawStream);
        using var dstRawStream = _fs.Open(dst, FileMode.Create, FileAccess.Write);
        using var dstStream = new GpgmeStreamData(dstRawStream);

        gpgContext.Context.Armor = true;

        // ReSharper disable once AccessToDisposedClosure
        using var ctRegister = cancellationToken.Register(() => gpgContext.Dispose());

        _logger.Information("Encryption of file {Source} to {Target}...", src, dst);
        try
        {
            gpgContext.Context.Encrypt(new[] { gpgContext.Key }, EncryptFlags.AlwaysTrust, srcStream, dstStream);
        }
        catch (Exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }

    public void Decrypt(string src, string dst, CancellationToken cancellationToken)
    {
        using var gpgContext = PrepareContext();

        using var srcRawStream = _fs.Open(src, FileMode.Open, FileAccess.Read);
        using var srcStream = new GpgmeStreamData(srcRawStream);
        using var dstRawStream = _fs.Open(dst, FileMode.Create, FileAccess.Write);
        using var dstStream = new GpgmeStreamData(dstRawStream);

        // ReSharper disable once AccessToDisposedClosure
        using var ctRegister = cancellationToken.Register(() => gpgContext.Dispose());
        
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