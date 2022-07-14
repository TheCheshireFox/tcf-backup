namespace TcfBackup.Managers;

public interface IEncryptionManager : IManager
{
    void Encrypt(string src, string dst);
    void Decrypt(string src, string dst);
}