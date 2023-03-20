namespace TcfBackup.Shared;

public class RingBufferStream : Stream
{
    private readonly object _lock = new();
    private readonly byte[] _buffer;
    private int _pos;
    private int _length;
    
    private readonly AutoResetEvent _spaceEvt = new(true);
    private readonly AutoResetEvent _dataEvt = new(false);
    private readonly CancellationTokenSource _cts = new();

    private void WaitEvent(WaitHandle evt, Func<bool> predicate)
    {
        var events = new[] { evt, _cts.Token.WaitHandle };
        
        while (!predicate())
        {
            Monitor.Exit(_lock);
            try
            {
                if (WaitHandle.WaitAny(events) == 1)
                {
                    throw new OperationCanceledException();
                }
            }
            finally
            {
                Monitor.Enter(_lock);
            }
        }
    }

    private int WriteNoLock(Span<byte> buffer)
    {
        if (buffer.Length > _buffer.Length - _length)
        {
            buffer = buffer[..(_buffer.Length - _length)];
        }

        if (_pos + _length + buffer.Length < _buffer.Length)
        {
            buffer.CopyTo(new Span<byte>(_buffer, _pos + _length, buffer.Length));
        }
        else
        {
            var part1 = buffer[..(_buffer.Length - (_pos + _length))];
            var part2 = buffer[part1.Length..];
        
            part1.CopyTo(new Span<byte>(_buffer, _pos + _length, part1.Length));
            part2.CopyTo(_buffer);
        }
        
        _length += buffer.Length;
        _dataEvt.Set();

        return buffer.Length;
    }

    private int ReadNoLock(Span<byte> buffer)
    {
        if (buffer.Length > _length)
        {
            buffer = buffer[.._length];
        }

        if (_pos + _length < _buffer.Length)
        {
            new Span<byte>(_buffer, _pos, buffer.Length).CopyTo(buffer);
        }
        else
        {
            var part1 = _buffer[_pos..];
            var part2 = _buffer[..(buffer.Length - part1.Length)];
            
            part1.CopyTo(buffer);
            part2.CopyTo(buffer[part1.Length..]);
        }

        _length -= buffer.Length;
        _pos += buffer.Length;

        if (_length == 0)
        {
            _pos = 0;
        }
        else if (_pos >= _buffer.Length)
        {
            _pos -= _buffer.Length;
        }

        _spaceEvt.Set();
        
        return buffer.Length;
    }
    
    public RingBufferStream(int bufferSize)
    {
        _buffer = new byte[bufferSize];
        _pos = 0;
        _length = 0;
    }

    public override void Flush()
    {
        
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        lock (_lock)
        {
            try
            {
                WaitEvent(_dataEvt, () => _length > 0);
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
            
            return ReadNoLock(new Span<byte>(buffer, offset, count));
        }
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        lock (_lock)
        {
            var bufferSpan = new Span<byte>(buffer, offset, count);
            while (bufferSpan.Length > 0)
            {
                try
                {
                    WaitEvent(_spaceEvt, () => _buffer.Length - _length > 0);
                }
                catch (OperationCanceledException)
                {
                    throw new ObjectDisposedException("Stream is closed");
                }
            
                var written = WriteNoLock(bufferSpan);
                bufferSpan = bufferSpan[written..];
            }
        }
    }

    public override void Close()
    {
        _cts.Cancel();
        base.Close();
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _length;
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
}