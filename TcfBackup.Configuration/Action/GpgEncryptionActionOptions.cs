namespace TcfBackup.Configuration.Action;

public class GpgEncryptionActionOptions : EncryptionActionOptions
{
    public string? KeyId { get; set; }
}