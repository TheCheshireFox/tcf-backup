namespace TcfBackup.Configuration.Action;

public enum EncryptionEngine
{
    None,
    Openssl,
    Gpg
}

public class EncryptionActionOptions : ActionOptions
{
    public EncryptionEngine Engine { get; set; }
    public string Password { get; set; } = string.Empty;
    public string KeyFile { get; set; } = string.Empty;
}