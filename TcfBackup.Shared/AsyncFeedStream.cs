namespace TcfBackup.Shared;

public class AsyncFeedStream : Stream
{
    private readonly Task _feedTask;
    private readonly CancellationTokenSource _cts = new();
    private readonly RingBufferStream _stream;

    private Exception? _feedTaskException;

    private void ThrowOnFailedTask()
    {
        if (_feedTaskException == null)
        {
            return;
        }

        throw _feedTaskException;
    }

    public AsyncFeedStream(Func<Stream, CancellationToken, Task> feedInitializer, int bufferSize, CancellationToken cancellationToken)
    {
        var token = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token;

        _stream = new RingBufferStream(bufferSize);
        _feedTask = feedInitializer(_stream, token).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                _feedTaskException = t.Exception ?? new Exception("Subsequent stream task failed");
                _cts.Cancel();
            }

            _stream.Close();

            return t;
        });
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowOnFailedTask();

        var read = _stream.Read(buffer, offset, count);
        if (read == 0)
        {
            ThrowOnFailedTask();
        }

        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ThrowOnFailedTask();

        try
        {
            _stream.Write(buffer, offset, count);
        }
        catch (ObjectDisposedException)
        {
            ThrowOnFailedTask();
            throw;
        }
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

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
}