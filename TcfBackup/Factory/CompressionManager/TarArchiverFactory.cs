using TcfBackup.Archiver;
using TcfBackup.Configuration.Action.CompressAction;
using TcfBackup.LibArchive.Tar;
using TcfBackup.Managers;

namespace TcfBackup.Factory.CompressionManager;

public class TarArchiverFactory : IArchiverFactory
{
    private readonly Func<Stream, IFilesArchiver> _filesFactory;

    public static TarArchiverFactory Create(TarCompressActionOptions tarOptions)
    {
        var mappedTarOptions = TarCompressOptionsMapper.Map(tarOptions);
        var mappedOptions = TarCompressOptionsMapper.Map(tarOptions.Options);

        return new TarArchiverFactory(s => new TarFilesArchiver(new TarLibArchiveStream(s, mappedTarOptions, mappedOptions)));
    }

    private TarArchiverFactory(Func<Stream, IFilesArchiver> filesFactory)
    {
        _filesFactory = filesFactory;
    }

    public IFilesArchiver CreateFilesArchiver(Stream archive) => _filesFactory(archive);
}