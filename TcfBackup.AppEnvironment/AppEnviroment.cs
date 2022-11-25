using TcfBackup.Filesystem;

namespace TcfBackup;

public static class AppEnvironment
{
    public const string TcfConfigDirectory = "/etc/tcf-backup";
    public const string TcfPersistentDirectory = "/var/lib/tcf-backup";
    public const string TcfDatabaseDirectory = $"{TcfPersistentDirectory}/db";
    
    public const string GlobalConfiguration = $"{TcfConfigDirectory}/global.yaml";

    public static void Initialize(IFileSystem fs)
    {
        fs.Directory.CreateDirectory(TcfConfigDirectory);
        fs.Directory.CreateDirectory(TcfPersistentDirectory);
        fs.Directory.CreateDirectory(TcfDatabaseDirectory);
    }
}