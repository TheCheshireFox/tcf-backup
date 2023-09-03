namespace TcfBackup.Managers.Gpg;

internal class ReaderStreamWrapper : BaseStreamWrapper
{
    public ReaderStreamWrapper(Stream readStream) : base(readStream)
    {
        if (!Stream.CanRead)
        {
            throw new NotSupportedException("Wrapper works only with CanWrite streams");
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Stream.Read(buffer, offset, count);
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;
}