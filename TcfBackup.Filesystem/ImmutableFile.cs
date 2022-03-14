namespace TcfBackup.Filesystem;

public class ImmutableFile : IFile
{
    private readonly IFilesystem _fs;

    public string Path { get; }

    public ImmutableFile(IFilesystem fs, string path)
    {
        Path = path;
        _fs = fs;
    }

    public void Copy(string destination, bool overwrite = false) => _fs.CopyFile(Path, destination, overwrite);
    public void Move(string destination, bool overwrite = false) => _fs.CopyFile(Path, destination, overwrite);

    public void Delete()
    {
    }
}