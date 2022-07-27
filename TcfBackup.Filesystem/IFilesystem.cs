namespace TcfBackup.Filesystem;

public interface IFilesystem
{
    IEnumerable<string> GetFiles(string directory, bool throwIfPermissionDenied = false, bool followSymlinks = false, bool oneFilesystem = false);

    string CreateTempDirectory();
    string CreateTempFile();
    string CreateTempFile(string filename, bool replace = false);

    void CreateDirectory(string path);

    bool FileExists(string? path);
    bool DirectoryExists(string? path);

    void Delete(string path);
    void Move(string source, string destination, bool overwrite);
    void CopyFile(string source, string destination, bool overwrite);
    void CopyDirectory(string source, string destination, bool recursive);

    string[] ReadAllLines(string path);
    string ReadAllText(string path);
    void WriteAllLines(string path, IEnumerable<string> lines);

    Stream OpenRead(string path);
    Stream OpenWrite(string path);
}