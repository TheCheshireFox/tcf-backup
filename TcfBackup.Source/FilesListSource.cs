using TcfBackup.Filesystem;

namespace TcfBackup.Source;

public class FilesListSource : ISource, IDisposable
{
    private readonly IFilesystem _fs;
    private readonly IEnumerable<IFile> _files;
    private readonly string? _parentDir;

    public FilesListSource(IFilesystem fs, IEnumerable<IFile> files, string? parentDir = null)
    {
        _fs = fs;
        _files = files;
        _parentDir = parentDir;
    }

    public static FilesListSource CreateMutable(IFilesystem fs, IEnumerable<string> files) => new(fs, files.Select(f => (IFile)new MutableFile(fs, f)));
    public static FilesListSource CreateImmutable(IFilesystem fs, IEnumerable<string> files) => new(fs, files.Select(f => (IFile)new ImmutableFile(fs, f)));

    public static FilesListSource CreateMutable(IFilesystem fs, string dir) => new(fs, fs.GetFiles(dir).Select(f => (IFile)new MutableFile(fs, f)), dir);
    
    public IEnumerable<IFile> GetFiles() => _files;

    public void Prepare()
    {
    }

    public void Cleanup()
    {
        foreach (var file in _files)
        {
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                // NOP
            }
        }

        if (_parentDir != null)
        {
            try
            {
                _fs.Delete(_parentDir);
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }

    public void Dispose() => Cleanup();
}