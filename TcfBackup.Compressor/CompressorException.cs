namespace TcfBackup.Compressor;

public class CompressorException : Exception
{
    protected CompressorException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
        
    }
}