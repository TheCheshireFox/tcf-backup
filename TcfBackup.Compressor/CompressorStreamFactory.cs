using TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;
using TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;
using TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma;

namespace TcfBackup.Compressor;

public class CompressorStreamFactory : ICompressorStreamFactory
{
    public Stream CreateGZip(GZipOptions options, Stream output) => new BlockCompressorStream(new GZipCompressor(options), output);

    public Stream CreateBZip2(BZip2Options options, Stream output) => new BlockCompressorStream(new BZip2Compressor(options), output);

    public Stream CreateXz(XzOptions options, Stream output) => new BlockCompressorStream(new XzCompressor(options), output);
}