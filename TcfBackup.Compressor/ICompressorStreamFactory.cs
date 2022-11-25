using TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;
using TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;
using TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma;

namespace TcfBackup.Compressor;

public interface ICompressorStreamFactory
{
    Stream CreateGZip(GZipOptions options, Stream output);
    Stream CreateBZip2(BZip2Options options, Stream output);
    Stream CreateXz(XzOptions options, Stream output);
}