using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Target;

public class DirTarget : ITarget
{
    private readonly ILogger _logger;
    private readonly bool _overwrite;

    public string Scheme => TargetSchemes.Filesystem;
    public string Directory { get; }

    public DirTarget(ILogger logger, IFileSystem filesystem, string dir, bool overwrite)
    {
        _logger = logger.ForContextShort<DirTarget>();
        _overwrite = overwrite;
        
        filesystem.Directory.CreateDirectory(Directory = dir);
        _logger.Information("Target directory {Dir}...", Directory);
    }

    public void Apply(ISource source, CancellationToken cancellationToken)
    {
        foreach (var file in source.GetFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dst = Path.Combine(Directory, Path.GetFileName(file.Path));
            _logger.Information("{Src} -> {Dst}...", file.Path, dst);
            file.Move(dst, _overwrite);
        }
        
        _logger.Information("Complete");
    }
}