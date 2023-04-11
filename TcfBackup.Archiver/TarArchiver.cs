using TcfBackup.LibArchive;

namespace TcfBackup.Archiver;

public class TarFilesArchiver : IFilesArchiver
{
    private readonly LibArchiveWriterBase _archiver;

    public event Action<LogLevel, string>? OnLog;

    public TarFilesArchiver(LibArchiveWriterBase archiver)
    {
        _archiver = archiver;
        _archiver.OnLog += (lvl, msg) => OnLog?.Invoke(lvl.ToLogLevel(), msg);
    }

    public void AddFile(string path, CancellationToken cancellationToken) => _archiver.AddFile(path, cancellationToken);
    
    public void Dispose() => _archiver.Dispose();
}