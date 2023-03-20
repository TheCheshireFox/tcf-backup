using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Target;

public class GDriveTarget : ITarget
{
    private readonly ILogger _logger;
    private readonly IGDriveAdapter _gDriveAdapter;
    private readonly IFileSystem _fs;
    private readonly string? _directory;
    private readonly string? _directoryId;

    public bool IsFilesystemTarget => false;
    public string Scheme => TargetSchemes.GDrive;
    public string Directory { get; }

    public GDriveTarget(ILogger logger, IGDriveAdapter gDriveAdapter, IFileSystem fs, string? path)
    {
        Directory = path ?? "/";
        
        _logger = logger.ForContextShort<GDriveTarget>();
        _gDriveAdapter = gDriveAdapter;
        _fs = fs;
        _directory = path;
        _directoryId = path != null ? _gDriveAdapter.CreateDirectory(path) : null;
    }
    
    public IEnumerable<string> Apply(IFileListSource source, CancellationToken cancellationToken)
    {
        var result = new List<string>();
        foreach (var file in source.GetFiles())
        {
            _logger.Information("Uploading {Path}...", file.Path);

            using var stream = _fs.File.Open(file.Path, FileMode.Open, FileAccess.Read);

            var name = Path.GetFileName(file.Path);
            _gDriveAdapter.UploadFile(stream, name, _directoryId, cancellationToken);

            result.Add(_directory != null
                ? Path.Combine(_directory, name)
                : name);
            
            _logger.Information("Complete");
        }

        return result;
    }

    public IEnumerable<string> Apply(IStreamSource source, CancellationToken cancellationToken)
    {
        _logger.Information("Uploading {Path}...", source.Name);
        
        _gDriveAdapter.UploadFile(source.GetStream(), source.Name, _directoryId, cancellationToken);
        
        _logger.Information("Complete");

        return new[]
        {
            _directory != null
                ? Path.Combine(_directory, source.Name)
                : source.Name
        };
    }
}