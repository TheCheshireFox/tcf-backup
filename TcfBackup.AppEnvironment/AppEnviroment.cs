using TcfBackup.Filesystem;

namespace TcfBackup;

public static class AppEnvironment
{
    public const string TcfConfigDirectory = "/etc/tcf-backup";
    public const string TcfPersistentDirectory = "/var/lib/tcf-backup";

    public static void Initialize(IFilesystem fs)
    {
        fs.CreateDirectory(TcfConfigDirectory);
        fs.CreateDirectory(TcfPersistentDirectory);
    }
}