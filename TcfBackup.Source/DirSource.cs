using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Source;

public class DirSource : IFileListSource
{
    private readonly ILogger _logger;
    private readonly IFileSystem _filesystem;
    private readonly string _dir;

    public DirSource(ILogger logger, IFileSystem filesystem, string dir)
    {
        _logger = logger.ForContextShort<DirSource>();
        _filesystem = filesystem;

        if (!_filesystem.Directory.Exists(dir))
        {
            throw new DirectoryNotFoundException(dir);
        }

        _dir = dir;
    }

    public IEnumerable<IFile> GetFiles() => GetFiles(false);
    public IEnumerable<IFile> GetFiles(bool followSymlinks) => _filesystem.Directory
        .GetFiles(_dir, recursive: true, sameFilesystem: true, skipAccessDenied: true, followSymlinks)
        .Select(f => (IFile)new ImmutableFile(_filesystem, f)).ToArray();

    public void Prepare()
    {
        _logger.Information("Prepared for listing files in directory {Dir}", _dir);
    }

    public void Cleanup()
    {
    }
}