using System.Runtime.InteropServices;

namespace TcfBackup.Compressor;

internal static class CompressorNative
{
    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_create")]
    public static extern bool Create(CompressorType type, string src, string dst, out IntPtr compressor);

    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_destroy")]
    public static extern void Destroy(IntPtr compressor);
    
    [DllImport("compressor", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compressor_get_last_error")]
    public static extern string GetLastError(IntPtr compressor);
}