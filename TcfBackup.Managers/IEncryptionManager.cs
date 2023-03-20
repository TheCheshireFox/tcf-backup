namespace TcfBackup.Managers;

public interface IEncryptionManager : IManager
{
    void Encrypt(Stream src, Stream dst, CancellationToken cancellationToken);
    void Encrypt(string src, string dst, CancellationToken cancellationToken);
    void Decrypt(string src, string dst, CancellationToken cancellationToken);
}