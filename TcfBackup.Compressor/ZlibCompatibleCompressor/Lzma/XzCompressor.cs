namespace TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma;

public class XzCompressor : ZlibCompatibleCompressor<XzOptions>
{
    public XzCompressor(XzOptions options, int bufferSize = 0) : base(new XzZlibCompatibleProcessor(), options, bufferSize)
    {
    }
}