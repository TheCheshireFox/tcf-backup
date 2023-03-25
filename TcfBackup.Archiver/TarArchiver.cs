using TcfBackup.LibArchive;

namespace TcfBackup.Archiver;

public class TarFilesArchiver : IFilesArchiver
{
    private readonly LibArchiveWriterBase _archiver;

    public TarFilesArchiver(LibArchiveWriterBase archiver)
    {
        _archiver = archiver;
    }

    public void AddFile(string path) => _archiver.AddFile(path);
    
    public void Dispose() => _archiver.Dispose();
}