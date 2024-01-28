namespace TcfBackup.Configuration.Action.EncryptAction;

public class OpensslEncryptActionOptions : EncryptActionOptions
{
    public string Cipher { get; set; } = string.Empty;
    public bool Salt { get; set; }
    public bool Pbkdf2 { get; set; }
    public int Iterations { get; set; } = 0;
}