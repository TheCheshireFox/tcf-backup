namespace TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;

public class BZip2Compressor : ZlibCompatibleCompressor<BZip2Options>
{
    public BZip2Compressor(BZip2Options options, int bufferSize = 0) : base(new BZip2ZlibCompatibleProcessor(), options, bufferSize)
    {
    }
}