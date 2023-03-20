using TcfBackup.LibArchive.Options;

namespace TcfBackup.LibArchive.Tar;

public class TarFileLibArchiveInitializer : ILibArchiveInitializer
{
    private readonly string _path;

    public TarFileLibArchiveInitializer(string path)
    {
        _path = path;
    }

    public void Initialize(nint archive)
    {
        LibArchiveNativeWrapper.archive_write_open_filename(archive, _path);
    }

    public void Cleanup(nint archive)
    {
        
    }
}

public class TarLibArchiveFile : TarLibArchiveBase
{
    public TarLibArchiveFile(string path, TarOptions tarOptions, OptionsBase options)
        : base(new TarFileLibArchiveInitializer(path), tarOptions, options)
    {

    }
}