namespace TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;

internal unsafe class BZip2ZlibCompatibleProcessor : IZlibCompatibleProcessor<BZip2Options>
{
    private BZStream _stream;

    public CompressionBuffer In
    {
        set
        {
            _stream.avail_in = (uint)value.Size;
            _stream.next_in = value.Buffer;
        }
    }

    public CompressionBuffer Out
    {
        get => new (_stream.next_out, (int)_stream.avail_out);
        
        set
        {
            _stream.avail_out = (uint)value.Size;
            _stream.next_out = value.Buffer;
        }
    }

    public void Init(BZip2Options options)
    {
        var ret = BZip2.CompressInit(ref _stream, options.BlockSize, options.Verbosity, options.WorkFactor);
        if (ret != BZipRet.Ok)
        {
            throw new BZip2Exception($"Unable to initialize compressor: {ret}");
        }
    }

    public void Cleanup()
    {
        var ret = BZip2.CompressEnd(ref _stream);
        if (ret != BZipRet.Ok)
        {
            throw new Exception(ret.ToString());
        }
    }

    public CompressStatus Compress() => BZip2.Compress(ref _stream, FlushFlag.Run) switch
    {
        BZipRet.StreamEnd => CompressStatus.Complete,
        BZipRet.RunOk => _stream.avail_in == 0 ? CompressStatus.Complete : CompressStatus.More,
        var error => throw new BZip2Exception(error.ToString())
    };

    public CompressStatus Flush() => BZip2.Compress(ref _stream, FlushFlag.Finish) switch
    {
        BZipRet.StreamEnd => CompressStatus.Complete,
        BZipRet.FinishOk => CompressStatus.More,
        var error => throw new BZip2Exception(error.ToString())
    };
}