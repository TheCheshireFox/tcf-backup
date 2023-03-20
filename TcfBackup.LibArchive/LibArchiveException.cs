namespace TcfBackup.LibArchive;

public class LibArchiveException : Exception
{
    public RetCode? RetCode { get; }

    public LibArchiveException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
        RetCode = null;
    }
    
    public LibArchiveException(RetCode retCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        RetCode = retCode;
    }
}