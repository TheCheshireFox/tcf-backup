using System.Text;
using Libgpgme;

namespace TcfBackup.Managers.Gpg;

public static class EncryptionResultExtension
{
    public static void ThrowOnError(this EncryptionResult? result)
    {
        if (result?.InvalidRecipients == null)
        {
            return;
        }
        
        var sb = new StringBuilder();

        foreach (var invalidKey in result.InvalidRecipients)
        {
            if (invalidKey == null)
            {
                continue;
            }

            sb.AppendLine($"{invalidKey.Fingerprint}: {Gpgme.GetStrError(invalidKey.Reason)}");
        }

        if (sb.Length == 0)
        {
            return;
        }

        throw new Exception(sb.ToString());
    }
}