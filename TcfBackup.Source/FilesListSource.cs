using TcfBackup.Filesystem;

namespace TcfBackup.Source;

public class FilesListSource : IFileListSource, IDisposable
{
    private readonly IFileSystem _fs;
    private readonly List<IFile> _files;
    private readonly string? _parentDir;

    public FilesListSource(IFileSystem fs, IEnumerable<IFile> files, string? parentDir = null)
    {
        _fs = fs;
        _files = files.ToList();
        _parentDir = parentDir;
    }

    public static FilesListSource CreateMutable(IFileSystem fs, string dir)
    {
        var files = fs.Directory.EnumerateFiles(dir, "*", new EnumerationOptions()
        {
            IgnoreInaccessible = false,
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false
        });
        return new FilesListSource(fs, files.Select(f => (IFile)new MutableFile(fs, f)), dir);
    }

    public static FilesListSource CreateImmutable(IFileSystem fs, IEnumerable<string> files)
    {
        return new FilesListSource(fs, files.Select(f => new ImmutableFile(fs, f)));
    }
    
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
                _fs.Directory.Delete(_parentDir, true);
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }

    public void Dispose() => Cleanup();
}