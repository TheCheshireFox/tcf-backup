using TcfBackup.LibArchive;

namespace TcfBackup.Archiver;

public class TarFilesArchiver : IFilesArchiver
{
    private readonly LibArchiveWriterBase _archiver;

    public event Action<LogLevel, string>? OnLog;

    public TarFilesArchiver(LibArchiveWriterBase archiver)
    {
        _archiver = archiver;
        _archiver.OnLog += (lvl, msg) => OnLog?.Invoke(lvl switch
        {
            LibArchive.LogLevel.Error => LogLevel.Error,
            LibArchive.LogLevel.Warning => LogLevel.Warning,
            _ => throw new ArgumentOutOfRangeException(nameof(lvl), lvl, null)
        }, msg);
    }

    public void AddFile(string path) => _archiver.AddFile(path);
    
    public void Dispose() => _archiver.Dispose();
}