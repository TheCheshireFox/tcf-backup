namespace TcfBackup.Compressor;

public class BlockCompressorStream : Stream
{
    private readonly IBlockCompressor _compressor;
    private readonly Stream _output;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        
        _compressor.Flush(_output);
        _compressor.Dispose();
    }
    
    public BlockCompressorStream(IBlockCompressor compressor, Stream output)
    {
        _compressor = compressor;
        _output = output;
    }

    public override void Flush()
    {
        
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value)  => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => _compressor.Compress(new Span<byte>(buffer, offset, count), _output);

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get  => throw new NotSupportedException(); set  => throw new NotSupportedException(); }
}