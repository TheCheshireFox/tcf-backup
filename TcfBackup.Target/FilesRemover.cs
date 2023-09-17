using TcfBackup.Filesystem;

namespace TcfBackup.Target;

internal class FilesRemover : IDisposable
{
    private readonly List<string> _files = new();
    private readonly Action<string> _remover;

    public FilesRemover(IFileSystem fs, params string[] paths)
    {
        _remover = fs.File.Delete;
        _files.AddRange(paths);
    }

    public FilesRemover(Action<string> remover, params string[] paths)
    {
        _remover = remover;
        _files.AddRange(paths);
    }

    public void Add(string path) => _files.Add(path);
    public void Commit() => _files.Clear();

    public void Dispose()
    {
        foreach (var file in _files)
        {
            try
            {
                _remover(file);
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }
}