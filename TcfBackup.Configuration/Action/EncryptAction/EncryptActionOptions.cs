namespace TcfBackup.Configuration.Action.EncryptAction;

public enum EncryptionEngine
{
    None,
    Openssl,
    Gpg
}

public class EncryptActionOptions : ActionOptions
{
    [Variant<GpgEncryptActionOptions>(EncryptionEngine.Gpg)]
    [Variant<OpensslEncryptActionOptions>(EncryptionEngine.Openssl)]
    public EncryptionEngine Engine { get; set; }
    public string Password { get; set; } = string.Empty;
    public string KeyFile { get; set; } = string.Empty;
}