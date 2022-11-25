using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;

namespace TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;

internal unsafe class GZipZlibCompatibleProcessor : IZlibCompatibleProcessor<GZipOptions>
{
    private IntPtr _streamPtr;
    private ZStream* _stream;

    public CompressionBuffer In
    {
        set
        {
            _stream->AvailIn = (uint)value.Size;
            _stream->NextIn = value.Buffer;
        }
    }

    public CompressionBuffer Out
    {
        get => new (_stream->NextOut, (int)_stream->AvailOut);
        
        set
        {
            _stream->AvailOut = (uint)value.Size;
            _stream->NextOut = value.Buffer;
        }
    }

    private CompressStatus ToStatus(ZRetCode ret) => ret switch
    {
        ZRetCode.StreamEnd => CompressStatus.Complete,
        ZRetCode.Ok => _stream->AvailIn == 0 ? CompressStatus.Complete : CompressStatus.More,
        ZRetCode.BufError => _stream->AvailIn == 0 || _stream->AvailOut == 0 ? CompressStatus.Complete : CompressStatus.More,
        var error => throw new GZipException(error.ToString())
    };
    
    public void Init(GZipOptions options)
    {
        _streamPtr = Marshal.AllocHGlobal(sizeof(ZStream));
        _stream = (ZStream*)_streamPtr.ToPointer();
        
        Unsafe.InitBlock(_stream, 0, (uint)sizeof(ZStream));
        
        var ret = GZip.deflateInit2_(_stream,
            options.Level,
            CompressionMethod.Deflated,
            WindowBits.DeflateWithGZipHeader,
            MemoryLevel.Max,
            Strategy.DefaultStrategy,
            GZip.Version,
            sizeof(ZStream));
        if (ret != ZRetCode.Ok)
        {
            throw new GZipException($"Unable to initialize compressor: {ret}");
        }
    }

    public void Cleanup()
    {
        if (_streamPtr == IntPtr.Zero)
        {
            return;
        }
        
        var ret = GZip.deflateEnd(_stream);
        
        Marshal.FreeHGlobal(_streamPtr);
        _streamPtr = IntPtr.Zero;
        _stream = null;
        
        if (ret != ZRetCode.Ok)
        {
            throw new GZipException(ret.ToString());
        }
    }

    public CompressStatus Compress() => GZip.deflate(_stream, ZFlushCode.NoFlush) switch
    {
        ZRetCode.Ok => _stream->AvailIn == 0 ? CompressStatus.Complete : CompressStatus.More,
        ZRetCode.BufError => _stream->AvailIn == 0 || _stream->AvailOut == 0 ? CompressStatus.Complete : CompressStatus.More,
        var error => throw new GZipException(error.ToString())
    };
    
    public CompressStatus Flush() => GZip.deflate(_stream, ZFlushCode.Finish) switch
    {
        ZRetCode.Ok => CompressStatus.More,
        ZRetCode.StreamEnd => CompressStatus.Complete,
        ZRetCode.BufError => CompressStatus.More,
        var error => throw new GZipException(error.ToString())
    };
}