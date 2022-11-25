namespace TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma;

internal unsafe class XzZlibCompatibleProcessor : IZlibCompatibleProcessor<XzOptions>
{
    private LzmaStream _stream;

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

    private CompressStatus ToState(LzmaRet ret) => ret switch
    {
        LzmaRet.LzmaStreamEnd or LzmaRet.LzmaOk => CompressStatus.Complete,
        var error => _stream.avail_out == 0 ? CompressStatus.More : throw new XzException(error.ToString())
    };

    public void Init(XzOptions options)
    {
        LzmaRet ret;
        if (options.Threads != null)
        {
            var mtOptions = new LzmaMtOptions
            {
                check = LzmaCheck.LzmaCheckCrc64,
                preset = (uint)options.Level,
                threads = options.Threads.Value,
                blockSize = options.BlockSize
            };

            ret = Xz.LzmaStreamEncoderMt(ref _stream, ref mtOptions);
        }
        else
        {
            ret = Xz.LzmaEasyEncoder(ref _stream, options.Level, LzmaCheck.LzmaCheckCrc64);
        }
        
        if (ret != LzmaRet.LzmaOk)
        {
            throw new XzException($"Unable to initialize compressor: {ret}");
        }
    }

    public void Cleanup() => Xz.LzmaEnd(ref _stream);
    public CompressStatus Compress() => ToState(Xz.LzmaCode(ref _stream, LzmaAction.LzmaRun));
    public CompressStatus Flush() => ToState(Xz.LzmaCode(ref _stream, LzmaAction.LzmaFinish));
}