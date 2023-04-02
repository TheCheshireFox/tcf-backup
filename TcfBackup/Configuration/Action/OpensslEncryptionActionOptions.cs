using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Action;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class OpensslEncryptionActionOptions : EncryptionActionOptions
{
    public string Cipher { get; set; } = string.Empty;
    public bool Salt { get; set; }
    public bool Pbkdf2 { get; set; }
    public int Iterations { get; set; } = 0;
}