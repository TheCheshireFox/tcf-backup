namespace TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;

public class GZipCompressor : ZlibCompatibleCompressor<GZipOptions>
{
    public GZipCompressor(GZipOptions options, int bufferSize = 0) : base(new GZipZlibCompatibleProcessor(), options, bufferSize)
    {
    }
}