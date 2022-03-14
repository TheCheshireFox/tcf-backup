namespace TcfBackup.Configuration.Action
{
    public class GpgEncryptionActionOptions : EncryptionActionOptions
    {
        public string? Signature { get; set; }
    }
}