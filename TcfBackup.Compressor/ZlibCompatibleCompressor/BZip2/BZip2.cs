using System.Runtime.InteropServices;

namespace TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct BZStream
{
    public byte *next_in;
    public uint avail_in;
    public uint total_in_lo32;
    public uint total_in_hi32;
    public byte *next_out;
    public uint avail_out;
    public uint total_out_lo32;
    public uint total_out_hi32;
    public void *state;
    public void * bzalloc;
    public void *bzfree;
    public void *opaque;
}

internal enum BZipRet
{
    Ok = 0,
    RunOk = 1,
    FlushOk = 2,
    FinishOk = 3,
    StreamEnd = 4,
    SequenceError = -1,
    ParamError = -2,
    MemError = -3,
    DataError = -4,
    DataErrorMagic = -5,
    IoError = -6,
    UnexpectedEof = -7,
    OutbuffFull = -8,
    ConfigError = -9
}

internal enum FlushFlag
{
    Run = 0,
    Finish = 2
}

internal static class BZip2
{
    [DllImport("libbz2.so.1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "BZ2_bzCompressInit")]
    public static extern BZipRet CompressInit(ref BZStream stream, int blockSize100K, int verbosity, int workFactor);

    [DllImport("libbz2.so.1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "BZ2_bzCompressEnd")]
    public static extern BZipRet CompressEnd(ref BZStream stream);

    [DllImport("libbz2.so.1", CallingConvention = CallingConvention.Cdecl, EntryPoint = "BZ2_bzCompress")]
    public static extern BZipRet Compress(ref BZStream stream, FlushFlag action);
}