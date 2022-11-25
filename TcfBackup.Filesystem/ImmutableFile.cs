namespace TcfBackup.Filesystem;

public class ImmutableFile : IFile
{
    private readonly IFileSystem _fs;

    public string Path { get; }

    public ImmutableFile(IFileSystem fs, string path)
    {
        Path = path;
        _fs = fs;
    }

    public void Copy(string destination, bool overwrite = false) => _fs.File.Copy(Path, destination, overwrite);
    public void Move(string destination, bool overwrite = false) => _fs.File.Copy(Path, destination, overwrite);

    public void Delete()
    {
    }
}