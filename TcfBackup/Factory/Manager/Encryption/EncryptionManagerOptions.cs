namespace TcfBackup.Factory.Manager.Encryption;

public class EncryptionManagerOptions
{
    public EncryptionManagerType Type { get; set; }
    public string Password { get; set; }
    public string KeyFile { get; set; }
    public string? Signature { get; set; }
}