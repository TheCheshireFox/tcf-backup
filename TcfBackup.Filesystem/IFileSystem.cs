namespace TcfBackup.Filesystem;

public interface IFilesystemFile
{
    bool Exists(string path);
    void Copy(string source, string destination, bool overwrite);
    void Move(string source, string destination, bool overwrite);
    void Delete(string path);
    
    Stream Open(string path, FileMode fileMode, FileAccess fileAccess);
    Stream Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
    Stream OpenRead(string path);
}

public interface IFilesystemDirectory
{
    bool Exists(string path);
    void Create(string path);
    void Delete(string path, bool recursive = true);
    
    IEnumerable<string> GetFiles(string path, bool recursive = true, bool sameFilesystem = true, bool skipAccessDenied = false, bool followSymlinks = false);
}

public interface IFileSystem
{
    IFilesystemFile File { get; }
    IFilesystemDirectory Directory { get; }

    string GetTempPath();
    string GetTempFileName();
}