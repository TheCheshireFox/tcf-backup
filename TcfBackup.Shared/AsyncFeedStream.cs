namespace TcfBackup.Shared;

public class AsyncFeedStream : Stream
{
    private readonly Task _feedTask;
    private readonly CancellationTokenSource _cts = new();
    private readonly RingBufferStream _stream;

    private void ThrowOnFailedTask()
    {
        if (!_feedTask.IsFaulted)
        {
            return;
        }
        
        _cts.Cancel();
        _stream.Close();
        throw _feedTask.Exception ?? new Exception("Subsequent stream task failed");
    }
    
    public AsyncFeedStream(Func<Stream, CancellationToken, Task> feedInitializer, int bufferSize)
    {
        _stream = new RingBufferStream(bufferSize);
        _feedTask = feedInitializer(_stream, _cts.Token);
    }
    
    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowOnFailedTask();
        return _stream.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ThrowOnFailedTask();
        _stream.Write(buffer, offset, count);
    }

    public override void Flush() => _stream.Flush();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Close()
    {
        _cts.Cancel();
        _stream.Close();

        try
        {
            _feedTask.ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch
        {
            // NOP
        }
        
        base.Close();
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _stream.Length;
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
}