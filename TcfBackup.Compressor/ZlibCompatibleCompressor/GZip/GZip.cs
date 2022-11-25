using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ZStream
{
    public byte* NextIn;
    public uint AvailIn;
    public ulong TotalIn;

    public byte* NextOut;
    public uint AvailOut;
    public ulong TotalOut;

    public byte* Msg;
    public void* State;

    public void* ZAlloc;
    public void* ZFree;
    public void* Opaque;

    public int DataType;

    public ulong Adler;
    public ulong Reserved;
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum ZRetCode
{
    Ok = 0,
    StreamEnd = 1,
    NeedDict = 2,
    Errno = -1,
    StreamError = -2,
    DataError = -3,
    MemError = -4,
    BufError = -5,
    VersionError = -6,
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum ZFlushCode
{
    NoFlush = 0,
    PartialFlush = 1,
    SyncFlush = 2,
    FullFlush = 3,
    Finish = 4,
    Block = 5,
    Trees = 6,
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum CompressionMethod
{
    Deflated = 8
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum WindowBits
{
    DeflateWithGZipHeader = 15 | 16
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum MemoryLevel
{
    Max = 9
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum Strategy
{
    Filtered = 1,
    HuffmanOnly = 2,
    Rle = 3,
    Fixed = 4,
    DefaultStrategy = 0
}

internal static unsafe class GZip
{
    public const string Version = "1";
    
    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflateInit2_(ZStream* stream, int level, CompressionMethod method, WindowBits windowBits, MemoryLevel memLevel, Strategy strategy, string version, int streamSize);

    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflateReset(ZStream* stream);

    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflateParams(ZStream* stream, int level, int strategy);

    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflatePending(ZStream* stream, IntPtr pending, out int bits);

    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflatePrime(ZStream* stream, int bits, int value);

    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflate(ZStream* stream, ZFlushCode flush);

    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflateSetDictionary(ZStream* stream, IntPtr dictionary, uint dictLength);

    [DllImport("libz.so.1", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ZRetCode deflateEnd(ZStream* stream);
}