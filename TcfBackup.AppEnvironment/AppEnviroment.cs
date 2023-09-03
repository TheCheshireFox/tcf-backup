using TcfBackup.Filesystem;

namespace TcfBackup;

public static class AppEnvironment
{
    public const string TcfConfigDirectory = "/etc/tcf-backup";
    public const string TcfPersistentDirectory = "/var/lib/tcf-backup";
    public const string TcfDatabaseDirectory = $"{TcfPersistentDirectory}/db";
    
    public const string GlobalConfiguration = $"{TcfConfigDirectory}/global.yaml";

    private static bool s_initialized;
    
    public static void Initialize(IFileSystem fs)
    {
        if (s_initialized)
        {
            return;
        }
        
        fs.Directory.Create(TcfConfigDirectory);
        fs.Directory.Create(TcfPersistentDirectory);
        fs.Directory.Create(TcfDatabaseDirectory);

        s_initialized = true;
    }
}