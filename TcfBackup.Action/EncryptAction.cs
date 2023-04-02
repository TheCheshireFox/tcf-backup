using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action;

public class EncryptAction : IAction
{
    private readonly ILogger _logger;
    private readonly IFileSystem _filesystem;
    private readonly IEncryptionManager _encryptionManager;

    private Task RunStreamEncryption(Stream src, Stream dst, CancellationToken cancellationToken)
    {
        void StreamEncryption()
        {
            _logger.Information("Encrypting stream...");
            _encryptionManager.Encrypt(src, dst, cancellationToken);
            _logger.Information("Encryption complete");
        }
        
        return Task.Factory.StartNew(StreamEncryption,
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Current);
    }
    
    public EncryptAction(ILogger logger, IFileSystem filesystem, IEncryptionManager encryptionManager)
    {
        _logger = logger.ForContextShort<EncryptAction>();
        _filesystem = filesystem;
        _encryptionManager = encryptionManager;
    }

    private Task ApplyAsync(IFileListSource source, IActionContext actionContext, CancellationToken cancellationToken)
    {
        var files = source.GetFiles().ToList();
        if (files.Count == 1)
        {
            return ApplyAsync(new FileStreamSource(_filesystem, (FileStream)_filesystem.File.OpenRead(files[0].Path), false), actionContext, cancellationToken);
        }
        
        _logger.Information("Start encryption");

        var targetDir = _filesystem.GetTempPath();

        try
        {
            var encryptedFiles = new Dictionary<string, string>();
            foreach (var file in source.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var dst = Path.Combine(targetDir, Path.GetFileName(file.Path));
                while (_filesystem.File.Exists(dst))
                {
                    dst = Path.Combine(targetDir, StringExtensions.GenerateRandomString(8) + PathUtils.GetFullExtension(file.Path));
                }

                encryptedFiles.Add(file.Path, dst);
            }

            foreach (var (src, dst) in encryptedFiles)
            {
                _encryptionManager.Encrypt(src, dst, cancellationToken);
            }

            actionContext.SetResult(FilesListSource.CreateMutable(_filesystem, targetDir));
            
            _logger.Information("Encryption complete");

            return Task.CompletedTask;
        }
        catch (Exception)
        {
            _filesystem.Directory.Delete(targetDir);
            throw;
        }
    }

    private Task ApplyAsync(IStreamSource source, IActionContext actionContext, CancellationToken cancellationToken)
    {
        var asyncStream = new AsyncFeedStream((dst, ct) => RunStreamEncryption(source.GetStream(), dst, ct), 1024 * 1024, cancellationToken);
        actionContext.SetResult(new StreamSource(asyncStream, source.Name));

        return Task.CompletedTask;
    }
    
    public Task ApplyAsync(IActionContext actionContext, CancellationToken cancellationToken)
    {
        return ActionContextExecutor
            .For(actionContext)
            .ApplyStreamSource(ApplyAsync)
            .ApplyFileListSource(ApplyAsync)
            .ExecuteAsync(cancellationToken);
    }
}