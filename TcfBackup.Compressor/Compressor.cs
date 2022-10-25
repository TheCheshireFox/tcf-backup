using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace TcfBackup.Compressor;

public delegate void OnCompressorLog(LogLevel level, string message);

public class CompressorLogger
{
    private static readonly Dictionary<int, LogLevel> s_compressorToLogLevelMapping = new()
    {
        { 0, LogLevel.Debug },
        { 1, LogLevel.Information },
        { 2, LogLevel.Warning },
        { 3, LogLevel.Error },
    };
    
    private static readonly Dictionary<LogLevel, int> s_logLevelToCompressorMapping = new()
    {
        { LogLevel.Trace, 0 },
        { LogLevel.Debug, 0 },
        { LogLevel.Information, 1 },
        { LogLevel.Warning, 2 },
        { LogLevel.Error, 3 },
        { LogLevel.Critical, 3 },
    };

    private static readonly CompressorNative.OnLog s_onLogInternalDelegate = OnLogInternal;
    private static void OnLogInternal(int level, string message) => OnLog?.Invoke(s_compressorToLogLevelMapping[level], message);

    public static event OnCompressorLog? OnLog;
    
    static CompressorLogger()
    {
        GCHandle.Alloc(s_onLogInternalDelegate);
        CompressorNative.SetLogging(s_logLevelToCompressorMapping[LogLevel.Critical], s_onLogInternalDelegate);
    }
    
    public static void SetLoggingLevel(LogLevel logLevel)
    {
        CompressorNative.SetLogging(s_logLevelToCompressorMapping[logLevel], s_onLogInternalDelegate);
    }
}

public unsafe class Compressor : IDisposable
{
    private IntPtr _pCompressor;
    
    public Compressor(CompressorType compressorType)
    {
        try
        {
            if (!CompressorNative.Create(compressorType, out _pCompressor))
            {
                var error = CompressorNative.GetLastError(_pCompressor);
                CompressorNative.Destroy(_pCompressor);
                throw new Exception(error);
            }

        }
        catch (Exception)
        {
            CompressorNative.Destroy(_pCompressor);
            throw;
        }
    }

    public void Compress(Span<byte> src, Stream dst, int bufferSize = 16 * 1024)
    {
        bool CompressOnce(byte* pSrc, int srcLength, byte* pDst, int dstSize)
        {
            switch (CompressorNative.Compress(_pCompressor, pSrc, srcLength, pDst, dstSize, out var compressedSize))
            {
                case CompressStatus.Complete:
                    dst.Write(new Span<byte>(pDst, compressedSize));
                    return true;
                case CompressStatus.More:
                    dst.Write(new Span<byte>(pDst, compressedSize));
                    return false;
                case CompressStatus.Error:
                    throw new Exception(CompressorNative.GetLastError(_pCompressor));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        var buffer = new byte[bufferSize];
        fixed (byte* pSrc = src)
        fixed (byte* pBuffer = buffer)
        {
            if (CompressOnce(pSrc, src.Length, pBuffer, buffer.Length))
            {
                return;
            }

            while (!CompressOnce(null, 0, pBuffer, buffer.Length))
            {

            }
        }
    }
    
    public void Cleanup(Stream dst, int bufferSize = 16 * 1024)
    {
        if (_pCompressor == IntPtr.Zero)
        {
            return;
        }
        
        var buffer = new byte[bufferSize];
        fixed (byte* pBuffer = buffer)
        {
            int compressedSize;
            while (CompressorNative.Cleanup(_pCompressor, pBuffer, buffer.Length, out compressedSize) == CompressStatus.More)
            {
                dst.Write(new Span<byte>(pBuffer, buffer.Length));
            }

            if (compressedSize > 0)
            {
                dst.Write(new Span<byte>(pBuffer, buffer.Length));
            }
        }
    }
    
    protected virtual void Dispose(bool disposing)
    {
        CompressorNative.Destroy(_pCompressor);
        _pCompressor = IntPtr.Zero;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Compressor()
    {
        Dispose(false);
    }
}