using System.Runtime.InteropServices;

namespace TcfBackup.Compressor;

public static unsafe class CompressorNative
{
    public delegate void OnLog(int level, string message);
    
    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "set_logging")]
    public static extern IntPtr SetLogging(int level, OnLog onLog);
    
    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_create")]
    public static extern bool Create(CompressorType type, out IntPtr compressor);
    
    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_compress")]
    public static extern CompressStatus Compress(IntPtr compressor, byte* src, int srcSize, byte* dst, int dstSize, out int compressedSize);

    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_cleanup")]
    public static extern CompressStatus Cleanup(IntPtr compressor, byte* dst, int dstSize, out int compressedSize);
    
    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_destroy")]
    public static extern void Destroy(IntPtr compressor);
    
    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_get_last_error")]
    public static extern string GetLastError(IntPtr compressor);
}