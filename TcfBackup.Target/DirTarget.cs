using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Target;

internal class FilesRemover : IDisposable
{
    private readonly List<string> _files = new();
    private readonly IFileSystem _fs;

    public FilesRemover(IFileSystem fs, params string[] paths)
    {
        _fs = fs;
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
                _fs.File.Delete(file);
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }
}

public class DirTarget : ITarget
{
    private readonly ILogger _logger;
    private readonly IFileSystem _filesystem;
    private readonly bool _overwrite;

    public string Scheme => TargetSchemes.Filesystem;
    public string Directory { get; }

    public DirTarget(ILogger logger, IFileSystem filesystem, string dir, bool overwrite)
    {
        _logger = logger.ForContextShort<DirTarget>();
        _filesystem = filesystem;
        _overwrite = overwrite;
        
        _filesystem.Directory.Create(Directory = dir);
        _logger.Information("Target directory {Dir}...", Directory);
    }
    
    public IEnumerable<string> Apply(IFileListSource source, CancellationToken cancellationToken)
    {
        using var filesRemover = new FilesRemover(_filesystem);
        
        var result = new List<string>();
        foreach (var file in source.GetFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dst = Path.Combine(Directory, Path.GetFileName(file.Path));
            _logger.Information("{Src} -> {Dst}...", file.Path, dst);
            file.Move(dst, _overwrite);
            
            filesRemover.Add(dst);
            result.Add(dst);
        }
        
        filesRemover.Commit();
        _logger.Information("Complete");

        return result;
    }

    public IEnumerable<string> Apply(IStreamSource source, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var dst = Path.Combine(Directory, source.Name);
        _logger.Information("Writing {Dst}...", dst);

        using var filesRemover = new FilesRemover(_filesystem, dst);
        
        using var fileStream = _filesystem.File.Open(dst, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        source.GetStream().CopyToAsync(fileStream, 1024 * 1024, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        
        _logger.Information("Complete");
        filesRemover.Commit();

        return new[] { dst };
    }
}