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
        fs.Directory.Create(TcfConfigDirectory);
        fs.Directory.Create(TcfPersistentDirectory);
        fs.Directory.Create(TcfDatabaseDirectory);
    }
}