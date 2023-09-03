namespace TcfBackup.Managers.Gpg;

// Quirk of libgpgme, GpgmeStreamData expects stream Position is implemented
internal class WriterStreamWrapper : BaseStreamWrapper
{
    private long _virtualPosition;
    private long _virtualLength;

    public WriterStreamWrapper(Stream writeStream) : base(writeStream)
    {
        if (!Stream.CanWrite)
        {
            throw new NotSupportedException("Wrapper works only with CanWrite streams");
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Stream.Write(buffer, offset, count);

        _virtualLength += count;
        _virtualPosition += count;
    }

    public override bool CanRead => false;
    public override bool CanWrite => true;
    public override long Length => _virtualLength;

    public override long Position
    {
        get => _virtualPosition;
        set => throw new NotSupportedException();
    }
}