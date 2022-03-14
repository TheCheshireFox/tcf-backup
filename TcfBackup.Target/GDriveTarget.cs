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

    public GDriveTarget(ILogger logger, IGDriveAdapter gDriveAdapter, IFilesystem fs, string? path)
    {
        _logger = logger.ForContextShort<GDriveTarget>();
        _gDriveAdapter = gDriveAdapter;
        _fs = fs;
        _directoryId = path != null ? _gDriveAdapter.CreateDirectory(path) : null;
    }

    public void Apply(ISource source)
    {
        foreach (var file in source.GetFiles())
        {
            _logger.Information("Uploading {path}...", file.Path);

            using var stream = _fs.OpenRead(file.Path);
            _gDriveAdapter.UploadFile(stream, Path.GetFileName(file.Path), _directoryId);

            _logger.Information("Complete");
        }
    }
}