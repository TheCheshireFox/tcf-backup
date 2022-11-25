namespace TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;

public class BZip2Exception : CompressorException
{
    public BZip2Exception(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}