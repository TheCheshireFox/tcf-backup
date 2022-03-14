namespace TcfBackup.Filesystem;

public interface IFile
{
    string Path { get; }

    void Copy(string destination, bool overwrite = false);
    void Move(string destination, bool overwrite = false);
    void Delete();
}