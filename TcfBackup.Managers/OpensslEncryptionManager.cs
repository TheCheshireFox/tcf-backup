using Serilog;
using TcfBackup.Shared;

namespace TcfBackup.Managers;

public class OpensslEncryptionManager : IEncryptionManager
{
    private readonly ILogger _logger;

    private readonly string _cipher;
    private readonly bool _salt;
    private readonly bool _pbkdf2;
    private readonly int _iterations;

    private readonly Func<string> _getPassArgument;

    public static OpensslEncryptionManager CreateWithKeyFile(ILogger logger, string keyfile, string cipher, bool salt, bool pbkdf2, int iterations = 0)
        => new(logger, () => $"file: {keyfile}", cipher, salt, pbkdf2, iterations);

    public static OpensslEncryptionManager CreateWithPassword(ILogger logger, string password, string cipher, bool salt, bool pbkdf2, int iterations = 0)
        => new(logger, () => $"pass: {password}", cipher, salt, pbkdf2, iterations);

    public OpensslEncryptionManager(ILogger logger, Func<string> getPassArgument, string cipher, bool salt, bool pbkdf2, int iterations = 0)
    {
        _logger = logger;
        _cipher = cipher;
        _salt = salt;
        _pbkdf2 = pbkdf2;
        _iterations = iterations;
        _getPassArgument = getPassArgument;

        if (Subprocess.Exec("openssl", "enc -ciphers")
            .Replace("\n", "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c[1..])
            .All(c => c != cipher))
        {
            throw new NotSupportedException($"Cipher {cipher} is not supported by openssl");
        }
    }

    private IEnumerable<string> BuildEncryptionSpecificArgs()
    {
        yield return $"-{_cipher}";
        if (_salt) yield return "-salt";
        if (_pbkdf2) yield return "-pbkdf2";
        if (_iterations > 0) yield return $"-iter {_iterations}";
    }

    public void Encrypt(string src, string dst)
    {
        var args = new List<string>
        {
            $"-in {src}",
            $"-out {dst}",
            $"-pass {_getPassArgument()}"
        };

        args.AddRange(BuildEncryptionSpecificArgs());

        Subprocess.Exec("openssl", "enc " + string.Join(' ', args), _logger.GetProcessRedirects());

        _logger.Information($"For decryption use: openssl enc -d -in <enc_file> -out <dec_file> -pass <pass: or file:> " + string.Join(' ', BuildEncryptionSpecificArgs()));
    }

    public void Decrypt(string src, string dst)
    {
        var args = new List<string>
        {
            $"-in {src}",
            $"-out {dst}",
            $"-pass {_getPassArgument()}"
        };

        args.AddRange(BuildEncryptionSpecificArgs());

        _logger.Information("Decrypting {src} to {dst}...", src, dst);
        Subprocess.Exec("openssl", "enc -d " + string.Join(' ', args), _logger.GetProcessRedirects());
        _logger.Information("Decryption complete");
    }
}