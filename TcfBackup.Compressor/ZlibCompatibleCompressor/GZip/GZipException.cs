namespace TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;

public class GZipException : CompressorException
{
    public GZipException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}