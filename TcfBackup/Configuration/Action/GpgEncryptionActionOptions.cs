using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Action;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class GpgEncryptionActionOptions : EncryptionActionOptions
{
    public string? KeyId { get; set; }
}