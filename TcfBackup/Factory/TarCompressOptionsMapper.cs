using BZip2Options = TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2.BZip2Options;
using GZipOptions = TcfBackup.Compressor.ZlibCompatibleCompressor.GZip.GZipOptions;
using XzOptions = TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma.XzOptions;

namespace TcfBackup.Factory;

public static class TarCompressOptionsMapper
{
    public static GZipOptions Map(TcfBackup.Configuration.Action.GZipOptions? opts) => new()
    {
        Level = opts?.Level ?? 6,
    };
    
    public static BZip2Options Map(TcfBackup.Configuration.Action.BZip2Options? opts) => new()
    {
        
    };
    
    public static XzOptions Map(TcfBackup.Configuration.Action.XzOptions? opts) => new()
    {
        Level = opts?.Level ?? 6,
        Threads = opts?.Threads,
        BlockSize = opts?.BlockSize ?? 0
    };
}