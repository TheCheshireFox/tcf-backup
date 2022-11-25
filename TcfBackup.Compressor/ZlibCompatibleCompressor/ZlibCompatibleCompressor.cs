namespace TcfBackup.Compressor.ZlibCompatibleCompressor;

public unsafe struct CompressionBuffer
{
    public readonly byte* Buffer;
    public readonly int Size;

    public CompressionBuffer(byte* buffer, int size)
    {
        Buffer = buffer;
        Size = size;
    }
}

public interface IZlibCompatibleProcessor<in TOptions>
{
    public CompressionBuffer In { set; }
    public CompressionBuffer Out { get; set; }

    void Init(TOptions options);
    void Cleanup();
    CompressStatus Compress();
    CompressStatus Flush();
}

public class ZlibCompatibleCompressor<TOptions> : IBlockCompressor
{
    private readonly IZlibCompatibleProcessor<TOptions> _processor;
    private readonly int _bufferSize;

    private unsafe void CompressAll(Span<byte> buffer, Stream destination, byte* pDst)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        fixed (byte* pSrc = buffer)
        {
            _processor.In = new CompressionBuffer(pSrc, buffer.Length);
            while (true)
            {
                _processor.Out = new CompressionBuffer(pDst, _bufferSize);

                var ret = _processor.Compress();

                if (_bufferSize - _processor.Out.Size > 0)
                {
                    destination.Write(new Span<byte>(pDst, _bufferSize - _processor.Out.Size));
                }

                if (ret == CompressStatus.Complete)
                {
                    break;
                }
            }
        }
    }

    private unsafe void FlushAll(Stream destination, byte* pDst)
    {
        while (true)
        {
            _processor.Out = new CompressionBuffer(pDst, _bufferSize);
            var flushRet = _processor.Flush();
            switch (flushRet)
            {
                case CompressStatus.Complete:
                    destination.Write(new Span<byte>(pDst, _bufferSize - _processor.Out.Size));
                    return;
                case CompressStatus.More:
                    destination.Write(new Span<byte>(pDst, _bufferSize - _processor.Out.Size));
                    continue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CompressStatus), flushRet, null);
            }
        }
    }

    protected ZlibCompatibleCompressor(IZlibCompatibleProcessor<TOptions> processor, TOptions options, int bufferSize = 0)
    {
        _processor = processor;
        _bufferSize = bufferSize == 0 ? 64 * 1024 : bufferSize;

        processor.Init(options);
    }

    public unsafe void Compress(Span<byte> buffer, Stream destination)
    {
        try
        {
            fixed (byte* pDst = new byte[_bufferSize])
            {
                CompressAll(buffer, destination, pDst);
            }
        }
        catch (Exception)
        {
            _processor.Cleanup();
            throw;
        }
    }

    public unsafe void Flush(Stream destination)
    {
        try
        {
            fixed (byte* pDst = new byte[_bufferSize])
            {
                FlushAll(destination, pDst);
            }
        }
        catch (Exception)
        {
            _processor.Cleanup();
            throw;
        }
    }

    public void Dispose()
    {
        _processor.Cleanup();
        GC.SuppressFinalize(this);
    }
}