namespace TcfBackup.Filesystem;

public class MutableFile : IFile
{
    private readonly IFileSystem _fs;

    public string Path { get; }

    public MutableFile(IFileSystem fs, string path)
    {
        Path = path;
        _fs = fs;
    }

    public void Copy(string destination, bool overwrite = false) => _fs.File.Copy(Path, destination, overwrite);
    public void Move(string destination, bool overwrite = false) => _fs.File.Move(Path, destination, overwrite);
    public void Delete() => _fs.File.Delete(Path);
}