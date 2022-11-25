using System.Runtime.InteropServices;

namespace TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma;

internal enum LzmaRet
{
    LzmaOk,
    LzmaStreamEnd,
    LzmaNoCheck,
    LzmaUnsupportedCheck,
    LzmaGetCheck,
    LzmaMemError,
    LzmaMemlimitError,
    LzmaFormatError,
    LzmaOptionsError,
    LzmaDataError,
    LzmaBufError,
    LzmaProgError
}

internal enum LzmaCheck
{
    LzmaCheckNone = 0,
    LzmaCheckCrc32 = 1,
    LzmaCheckCrc64 = 4,
    LzmaCheckSha256 = 10
}

internal enum LzmaAction
{
    LzmaRun = 0,
    LzmaSyncFlush = 1,
    LzmaFullFlush = 2,
    LzmaFullBarrier = 4,
    LzmaFinish = 3
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct LzmaStream
{
    public byte* next_in;
    public ulong avail_in;
    public ulong total_in;
    public byte* next_out;
    public ulong avail_out;
    public ulong total_out;
    public void* allocator;
    public void* @internal;
    public void* reserved_ptr1;
    public void* reserved_ptr2;
    public void* reserved_ptr3;
    public void* reserved_ptr4;
    public ulong reserved_int1;
    public ulong reserved_int2;
    public ulong reserved_int3;
    public ulong reserved_int4;
    public int reserved_enum1;
    public int reserved_enum2;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct LzmaMtOptions
{
    public uint flags;
    public uint threads;
    public ulong blockSize;
    public uint timeout;
    public uint preset;
    public void* filters;
    public LzmaCheck check;
    public int reserved_enum1;
    public int reserved_enum2;
    public int reserved_enum3;
    public uint reserved_int1;
    public uint reserved_int2;
    public uint reserved_int3;
    public uint reserved_int4;
    public ulong reserved_int5;
    public ulong reserved_int6;
    public ulong reserved_int7;
    public ulong reserved_int8;
    public void *reserved_ptr1;
    public void *reserved_ptr2;
    public void *reserved_ptr3;
    public void *reserved_ptr4;
}

internal static class Xz
{
    [DllImport("liblzma.so.5", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzma_easy_encoder")]
    public static extern LzmaRet LzmaEasyEncoder(ref LzmaStream stream, int preset, LzmaCheck check);
    
    [DllImport("liblzma.so.5", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzma_stream_encoder_mt")]
    public static extern LzmaRet LzmaStreamEncoderMt(ref LzmaStream stream, ref LzmaMtOptions options);

    [DllImport("liblzma.so.5", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzma_end")]
    public static extern void LzmaEnd(ref LzmaStream stream);

    [DllImport("liblzma.so.5", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzma_code")]
    public static extern LzmaRet LzmaCode(ref LzmaStream stream, LzmaAction action);
}