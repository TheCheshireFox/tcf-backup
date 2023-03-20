using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Target;

public class DirTarget : ITarget
{
    private readonly ILogger _logger;
    private readonly IFileSystem _filesystem;
    private readonly bool _overwrite;

    public bool IsFilesystemTarget => true;
    public string Scheme => TargetSchemes.Filesystem;
    public string Directory { get; }

    public DirTarget(ILogger logger, IFileSystem filesystem, string dir, bool overwrite)
    {
        _logger = logger.ForContextShort<DirTarget>();
        _filesystem = filesystem;
        _overwrite = overwrite;
        
        _filesystem.Directory.CreateDirectory(Directory = dir);
        _logger.Information("Target directory {Dir}...", Directory);
    }
    
    public IEnumerable<string> Apply(IFileListSource source, CancellationToken cancellationToken)
    {
        var result = new List<string>();
        foreach (var file in source.GetFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dst = Path.Combine(Directory, Path.GetFileName(file.Path));
            _logger.Information("{Src} -> {Dst}...", file.Path, dst);
            file.Move(dst, _overwrite);
            
            result.Add(dst);
        }
        
        _logger.Information("Complete");

        return result;
    }

    public IEnumerable<string> Apply(IStreamSource source, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var dst = Path.Combine(Directory, source.Name);
        _logger.Information("Writing {Dst}...", dst);

        using var fileStream = _filesystem.File.Open(dst, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        source.GetStream().CopyToAsync(fileStream, 1024 * 1024, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        return new[] { dst };
    }
}