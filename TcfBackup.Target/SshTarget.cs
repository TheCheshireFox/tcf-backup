using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Target;

public class SshTarget : ITarget
{
    private readonly ILogger _logger;
    private readonly IFileSystem _filesystem;
    private readonly ISshManager _sshManager;
    private readonly bool _overwrite;

    public string Scheme => TargetSchemes.Ssh;
    public string Directory { get; }

    public SshTarget(ILogger logger, IFileSystem filesystem, ISshManager sshManager, string dir, bool overwrite)
    {
        _logger = logger.ForContextShort<SshTarget>();
        _filesystem = filesystem;
        _sshManager = sshManager;
        _overwrite = overwrite;

        Directory = dir;
        
        _logger.Information("Target directory {Dir}...", Directory);
    }
    
    public IEnumerable<string> Apply(IFileListSource source, CancellationToken cancellationToken)
    {
        using var filesRemover = CreateFilesRemover(cancellationToken);
        
        var result = new List<string>();
        foreach (var file in source.GetFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dst = Path.Combine(Directory, Path.GetFileName(file.Path));
            _logger.Information("{Src} -> {Dst}...", file.Path, dst);

            using var stream = _filesystem.File.OpenRead(file.Path);
            _sshManager.Upload(stream, dst, _overwrite, cancellationToken);
            
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
        _logger.Information("Writing {Src} to {Dst}...", source.Name, dst);

        using var filesRemover = CreateFilesRemover(cancellationToken);
        filesRemover.Add(dst);
        
        _sshManager.Upload(source.GetStream(), dst, _overwrite, cancellationToken);
        
        _logger.Information("Complete");
        filesRemover.Commit();

        return new[] { dst };
    }

    private FilesRemover CreateFilesRemover(CancellationToken cancellationToken)
    {
        return new FilesRemover(f => _sshManager.Delete(f, cancellationToken.IsCancellationRequested
            ? CancellationToken.None
            : cancellationToken));
    }
}