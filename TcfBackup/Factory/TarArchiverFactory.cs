using System;
using System.IO;
using TcfBackup.Archiver;
using TcfBackup.Configuration.Action;
using TcfBackup.LibArchive.Tar;
using TcfBackup.Managers;
using TarOptions = TcfBackup.Configuration.Action.TarOptions;

namespace TcfBackup.Factory;

public class TarArchiverFactory : IArchiverFactory
{
    private readonly Func<Stream, IFilesArchiver> _filesFactory;

    private static TarArchiverFactory Create(TarOptions tarOptions, ICompressorOptions? options)
    {
        var mappedTarOptions = TarCompressOptionsMapper.Map(tarOptions);
        var mappedOptions = TarCompressOptionsMapper.Map(options);

        return new TarArchiverFactory(s => new TarFilesArchiver(new TarLibArchiveStream(s, mappedTarOptions, mappedOptions)));
    }

    private TarArchiverFactory(Func<Stream, IFilesArchiver> filesFactory)
    {
        _filesFactory = filesFactory;
    }
    
    public static TarArchiverFactory CreateGZip2(TarOptions tarOptions, GZipOptions? gZipOptions) => Create(tarOptions, gZipOptions);
    public static TarArchiverFactory CreateBZip(TarOptions tarOptions, BZip2Options? bZip2Options) => Create(tarOptions, bZip2Options);
    public static TarArchiverFactory CreateXz(TarOptions tarOptions, XzOptions? xzOptions) => Create(tarOptions, xzOptions);

    public IFilesArchiver CreateFilesArchiver(Stream archive) => _filesFactory(archive);
}