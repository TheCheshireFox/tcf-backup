using Serilog;
using TcfBackup.Managers;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action;

public class CompressAction : IAction
{
    private readonly ILogger _logger;
    private readonly ICompressionManager _compressionManager;
    private readonly string? _archiveName;

    public CompressAction(ILogger logger, ICompressionManager compressionManager, string? archiveName)
    {
        _logger = logger.ForContextShort<CompressAction>();
        _compressionManager = compressionManager;
        _archiveName = archiveName;
    }
    
    private static string ToExtension(CompressAlgorithm compressAlgorithm) => compressAlgorithm switch
    {
        CompressAlgorithm.Gzip => ".tar.gz",
        CompressAlgorithm.Xz => ".tar.xz",
        CompressAlgorithm.BZip2 => ".tar.bz2",
        _ => string.Empty
    };
    
    private static Task RunCompression(System.Action compressionAction, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(compressionAction,
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Current);
    }
    
    private Task RunStreamCompression(IEnumerable<string> files, Stream dst, CancellationToken cancellationToken)
    {
        return RunCompression(() =>
        {
            _logger.Information("Compressing files...");
            _compressionManager.Compress(dst, files, cancellationToken);
            _logger.Information("Compression complete");
        }, cancellationToken);
    }

    private void Apply(IFileListSource source, IActionContext actionContext, CancellationToken cancellationToken)
    {
        var archiveName = string.IsNullOrEmpty(_archiveName)
            ? $"{StringExtensions.GenerateRandomString(8)}.{ToExtension(_compressionManager.CompressAlgorithm)}"
            : string.IsNullOrEmpty(PathUtils.GetFullExtension(_archiveName))
                ? $"{_archiveName}.{ToExtension(_compressionManager.CompressAlgorithm)}"
                : _archiveName;
        
        var files = source.GetFiles();
        var asyncStream = new AsyncFeedStream((dst, ct) => RunStreamCompression(files.Select(f => f.Path), dst, ct), 1024 * 1024);
        
        actionContext.SetResult(new StreamSource(asyncStream, archiveName));
    }

    public void Apply(IActionContext actionContext, CancellationToken cancellationToken)
    {
        ActionContextExecutor
            .For(actionContext)
            .ApplyFileListSource(Apply)
            .Execute(cancellationToken);
    }
}