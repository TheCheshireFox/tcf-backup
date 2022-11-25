namespace TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma;

public class XzException : CompressorException
{
    public XzException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}