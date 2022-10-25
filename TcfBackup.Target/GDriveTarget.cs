using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Target;

public class GDriveTarget : ITarget
{
    private readonly ILogger _logger;
    private readonly IGDriveAdapter _gDriveAdapter;
    private readonly IFilesystem _fs;
    private readonly string? _directoryId;

    public string Scheme => TargetSchemes.GDrive;
    public string Directory { get; }
    
    public GDriveTarget(ILogger logger, IGDriveAdapter gDriveAdapter, IFilesystem fs, string? path)
    {
        Directory = path ?? "/";
        
        _logger = logger.ForContextShort<GDriveTarget>();
        _gDriveAdapter = gDriveAdapter;
        _fs = fs;
        _directoryId = path != null ? _gDriveAdapter.CreateDirectory(path) : null;
    }

    public void Apply(ISource source, CancellationToken cancellationToken)
    {
        foreach (var file in source.GetFiles())
        {
            _logger.Information("Uploading {Path}...", file.Path);

            using var stream = _fs.Open(file.Path, FileMode.Open, FileAccess.Read);
            _gDriveAdapter.UploadFile(stream, Path.GetFileName(file.Path), _directoryId, cancellationToken);

            _logger.Information("Complete");
        }
    }
}