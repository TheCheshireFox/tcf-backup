namespace TcfBackup.Filesystem;

public class MutableFile : IFile
{
    private readonly IFilesystem _fs;

    public string Path { get; }

    public MutableFile(IFilesystem fs, string path)
    {
        Path = path;
        _fs = fs;
    }

    public void Copy(string destination, bool overwrite = false) => _fs.CopyFile(Path, destination, overwrite);
    public void Move(string destination, bool overwrite = false) => _fs.Move(Path, destination, overwrite);
    public void Delete() => _fs.Delete(Path);
}