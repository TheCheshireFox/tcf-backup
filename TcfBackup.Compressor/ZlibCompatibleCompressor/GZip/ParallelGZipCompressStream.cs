using System.Buffers;
using System.Collections.Concurrent;

namespace TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;

internal class MemoryStreamPool : IDisposable
{
    private class MemoryStreamHolder
    {
        public MemoryStream MemoryStream { get; init; }
        public bool Free { get; set; }
    }
    
    private readonly List<MemoryStreamHolder> _memoryStreams = new();
    private readonly object _lock = new();
    private readonly int _initialSize;
    private readonly int _limit;

    private volatile bool _disposing;

    private MemoryStreamHolder? TryGetOrCreateMemoryStreamUnlocked()
    {
        var freeMemoryStream = _memoryStreams
            .Where(b => b.Free)
            .DefaultIfEmpty()
            .MaxBy(b => b?.MemoryStream.Capacity ?? 0);
        
        if (freeMemoryStream != null)
        {
            freeMemoryStream.Free = false;
            return freeMemoryStream;
        }

        if (_limit != -1 && _memoryStreams.Count == _limit)
        {
            return null;
        }

        var memoryStreamHolder = new MemoryStreamHolder
        {
            MemoryStream = new MemoryStream(_initialSize),
            Free = false
        };
        _memoryStreams.Add(memoryStreamHolder);

        return memoryStreamHolder;

    }

    private void WaitForMemoryStream()
    {
        Monitor.Wait(_lock);
        if (_disposing)
        {
            throw new OperationCanceledException();
        }
    }
    
    public MemoryStreamPool(int initialSize, int limit = -1)
    {
        _initialSize = initialSize;
        _limit = limit;
    }

    public MemoryStream Allocate()
    {
        if (_disposing)
        {
            throw new ObjectDisposedException(nameof(MemoryStreamPool));
        }
        
        lock (_lock)
        {
            while (true)
            {
                var memoryStreamHolder = TryGetOrCreateMemoryStreamUnlocked();
                if (memoryStreamHolder != null)
                {
                    memoryStreamHolder.MemoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStreamHolder.MemoryStream;
                }

                WaitForMemoryStream();
            }
        }
    }

    public void Free(MemoryStream memoryStream)
    {
        if (_disposing)
        {
            return;
        }
        
        lock (_lock)
        {
            var memoryStreamHolder = _memoryStreams.FirstOrDefault(b => b.MemoryStream == memoryStream);
            if (memoryStreamHolder == null)
            {
                throw new KeyNotFoundException();
            }

            memoryStreamHolder.Free = true;
            Monitor.Pulse(_lock);
        }
    }

    public void Dispose()
    {
        _disposing = true;
        lock (_lock)
        {
            Monitor.Pulse(_lock);
        }
    }
}

internal class BlockingSequencedCollection<T>
{
    private readonly object _lock = new();
    private readonly SortedList<long, T> _list = new();
    private readonly AutoResetEvent _newItemEvt = new(false);
    private readonly AutoResetEvent _takeItemEvt = new(false);
    private readonly int _boundedCapacity;

    private volatile bool _completed;
    private long _seqNum;

    public BlockingSequencedCollection(int boundedCapacity)
    {
        _boundedCapacity = boundedCapacity;
    }

    public void Add(T value, long sequenceNumber)
    {
        lock (_lock)
        {
            while (_list.Count == _boundedCapacity)
            {
                Monitor.Exit(_lock);
                _takeItemEvt.WaitOne();
                Monitor.Enter(_lock);
            }

            _list.Add(sequenceNumber, value);
            _newItemEvt.Set();
        }
    }

    public T? Take()
    {
        lock (_lock)
        {
            T value;
            while (!_list.TryGetValue(_seqNum, out value!))
            {
                if (_completed)
                {
                    return default;
                }
                
                Monitor.Exit(_lock);
                _newItemEvt.WaitOne();
                Monitor.Enter(_lock);
            }

            _takeItemEvt.Set();

            _seqNum++;
            return value;
        }
    }

    public void CompleteAdding()
    {
        _completed = true;
    }
}

internal class CompressJob
{
    public long SequenceNumber { get; init; }
    public bool More { get; init; }
    public MemoryStream In { get; init; }
}

internal class WriteJob
{
    private readonly ManualResetEvent _crcReady = new(false);
    private ulong _crc;
    
    public long InLength { get; init; }
    public MemoryStream Out { get; init; }

    public ulong Crc
    {
        get
        {
            _crcReady.WaitOne();
            return Volatile.Read(ref _crc);
        }
        set
        {
            Volatile.Write(ref _crc, value);
            _crcReady.Set();
        }
    }
}

public class ParallelGZipCompressStream : Stream
{
    private const int CompressBufferSize = 16384;
    
    private readonly Thread[] _compressThreads;
    private readonly Thread _writeThread;

    private readonly MemoryStreamPool _memoryStreamPool;
    private readonly BlockingCollection<CompressJob> _compressJobs;
    private readonly BlockingSequencedCollection<WriteJob> _writeJobs;
    private readonly CancellationTokenSource _cts = new();

    private readonly GZipOptions _options;

    private readonly Stream _stream;

    private long _writeJobSeqNumber;

    private static void WriteGZipHeader(Stream stream, int level)
    {
        stream.WriteByte(0x1f); // header
        stream.WriteByte(0x8b); // header
        stream.WriteByte(0x08); // deflate
        stream.WriteByte(0x00); // flags
        stream.Write(BitConverter.GetBytes((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds())); // mtime
        stream.WriteByte(level == 1 ? (byte)0x04 : (byte)0x02); // extra flag
        stream.WriteByte(0x03); // OS Unix
    }
    
    private static void WriteGZipTrailer(Stream stream, ulong crc, ulong uncompressedLength)
    {
        stream.Write(BitConverter.GetBytes((int)crc));
        stream.Write(BitConverter.GetBytes((int)uncompressedLength));
    }
    
    private static unsafe void CompressAll(ZStream* stream, Stream dst, ZFlushCode flushCode)
    {
        fixed (byte* buffer = new byte[CompressBufferSize])
        {
            while (true)
            {
                stream->NextOut = buffer;
                stream->AvailOut = CompressBufferSize;

                GZip.deflate(stream, flushCode);

                if (CompressBufferSize - stream->AvailOut > 0)
                {
                    dst.Write(new Span<byte>(buffer, (int)(CompressBufferSize - stream->AvailOut)));
                }

                if (stream->AvailOut > 0)
                {
                    break;
                }
            }

            if (stream->AvailIn != 0)
            {
                throw new GZipException("");
            }
        }
    }
    
    private unsafe void CompressThread()
    {
        var zStream = new ZStream();

        var ret = GZip.deflateInit2_(&zStream,
            _options.Level,
            CompressionMethod.Deflated,
            WindowBits.Deflate,
            MemoryLevel.Max,
            Strategy.DefaultStrategy,
            GZip.Version,
            sizeof(ZStream));
        if (ret != ZRetCode.Ok)
        {
            throw new GZipException($"Unable to initialize compressor: {ret}");
        }

        while (!_cts.IsCancellationRequested)
        {
            CompressJob job;
            try
            {
                job = _compressJobs.Take(_cts.Token);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            GZip.deflateReset(&zStream);
            GZip.deflateParams(&zStream, _options.Level, Strategy.DefaultStrategy);

            var outMemoryStream = _memoryStreamPool.Allocate();

            fixed (byte* src = new Span<byte>(job.In.GetBuffer(), 0, (int)job.In.Length))
            {
                zStream.NextIn = src;
                zStream.AvailIn = (uint)job.In.Length;
                
                if (job.More)
                {
                    CompressAll(&zStream, outMemoryStream, ZFlushCode.Block);
                
                    GZip.deflatePending(&zStream, IntPtr.Zero, out var bits);
                    if ((bits & 1) != 0)
                    {
                        CompressAll(&zStream, outMemoryStream, ZFlushCode.SyncFlush);
                    }
                    else if ((bits & 7) != 0)
                    {
                        while ((bits & 7) != 0)
                        {
                            if (GZip.deflatePrime(&zStream, 10, 2) != ZRetCode.Ok)
                            {
                                throw new GZipException("");
                            }
                            GZip.deflatePending(&zStream, IntPtr.Zero, out bits);
                        }
                    }
                
                    CompressAll(&zStream, outMemoryStream, ZFlushCode.FullFlush);
                }
                else
                {
                    CompressAll(&zStream, outMemoryStream, ZFlushCode.Finish);
                }
            }
            
            var writeJob = new WriteJob
            {
                Out = outMemoryStream,
                InLength = job.In.Length
            };
            
            _writeJobs.Add(writeJob, job.SequenceNumber);

            var crc = GZip.crc32_z(0, null, 0);
            fixed (byte* src = job.In.GetBuffer())
            {
                crc = GZip.crc32_z(crc, src, (ulong)job.In.Length);
            }

            writeJob.Crc = crc;
        }

        GZip.deflateEnd(&zStream);
    }

    private unsafe void WriteThread()
    {
        WriteGZipHeader(_stream, _options.Level);

        var uncompressedLength = 0L;
        var crc = GZip.crc32_z(0, null, 0);
        while (!_cts.IsCancellationRequested)
        {
            var job = _writeJobs.Take();
            if (job == null)
            {
                break;
            }

            uncompressedLength += job.InLength;
            job.Out.CopyTo(_stream, 64 * 1024);

            crc = GZip.crc32_combine(crc, job.Crc, job.InLength);
            
            _memoryStreamPool.Free(job.Out);
        }
        
        WriteGZipTrailer(_stream, crc, (ulong)uncompressedLength);
    }
    
    public ParallelGZipCompressStream(GZipOptions options, Stream stream, int threads)
    {
        _stream = stream;
        _options = options;
        
        _memoryStreamPool = new MemoryStreamPool(64 * 1024, threads + 1);
        _compressJobs = new BlockingCollection<CompressJob>(threads * 2);
        _writeJobs = new BlockingSequencedCollection<WriteJob>(threads * 2);

        _writeThread = new Thread(WriteThread);
        _compressThreads = Enumerable
            .Range(0, threads)
            .Select(_ => new Thread(CompressThread))
            .ToArray();
    }
    
    public override void Write(byte[] buffer, int offset, int count)
    {
        var memoryStream = _memoryStreamPool.Allocate();
        memoryStream.Write(buffer, offset, count);
        
        _compressJobs.Add(new CompressJob
        {
            In = memoryStream,
            More = false,
            SequenceNumber = _writeJobSeqNumber++
        });
    }
    
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        
        _cts.Cancel();
        foreach (var th in _compressThreads)
        {
            th.Join();
        }

        _writeThread.Join();
    }

    public override void Flush()
    {
        
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length  => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
}