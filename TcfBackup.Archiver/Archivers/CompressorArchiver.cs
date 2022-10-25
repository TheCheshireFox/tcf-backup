using TcfBackup.Compressor;

namespace TcfBackup.Archiver.Archivers;

public abstract class CompressorArchiver  : IStreamingArchiver
{
    private class StreamingStream : Stream
    {
        public delegate void OnWriteEvent(byte[] buffer, int offset, int count);

        public event OnWriteEvent OnWrite = null!;

        public override void Write(byte[] buffer, int offset, int count) => OnWrite(buffer, offset, count);
        
        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)  => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    }
    
    private readonly Compressor.Compressor _compressor;
    private readonly StreamingStream _input;

    public Stream Output { get; }
    public Stream Input => _input;

    private void OnWrite(byte[] buffer, int offset, int count) => _compressor.Compress(new Span<byte>(buffer, offset, count), Output);
    
    protected CompressorArchiver(CompressorType compressorType, Stream output)
    {
        _compressor = new Compressor.Compressor(compressorType);
        
        _input = new StreamingStream();
        _input.OnWrite += OnWrite;
        
        Output = output;
    }

    public void Dispose()
    {
        _compressor.Cleanup(Output);
        _compressor.Dispose();
        GC.SuppressFinalize(this);
    }
}