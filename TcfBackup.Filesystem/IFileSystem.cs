namespace TcfBackup.Filesystem;

public interface IFileSystemFile
{
    bool Exists(string path);
    void Copy(string source, string destination, bool overwrite);
    void Move(string source, string destination, bool overwrite);
    void Delete(string path);
    
    Stream Open(string path, FileMode fileMode, FileAccess fileAccess);
    Stream Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
    Stream OpenRead(string path);
}

public interface IFileSystemDirectory
{
    bool Exists(string? path);
    void Create(string path);
    void Delete(string path, bool recursive = true);
    
    IEnumerable<string> GetFiles(string path, bool recursive = true, bool sameFilesystem = true, bool skipAccessDenied = false, bool followSymlinks = false);
}

public interface IFileSystem
{
    IFileSystemFile File { get; }
    IFileSystemDirectory Directory { get; }

    string GetTempPath();
    string GetTempFileName();
}