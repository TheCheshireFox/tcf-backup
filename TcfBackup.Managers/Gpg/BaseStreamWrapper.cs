namespace TcfBackup.Managers.Gpg;

internal abstract class BaseStreamWrapper : Stream
{
    protected readonly Stream Stream;

    protected BaseStreamWrapper(Stream stream)
    {
        Stream = stream;
    }

    public override void Flush() => Stream.Flush();
    public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);
    public override void SetLength(long value) => Stream.SetLength(value);

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override bool CanSeek => Stream.CanSeek;
    public override long Length => Stream.Length;

    public override long Position
    {
        get => Stream.Position;
        set => Stream.Position = value;
    }
}