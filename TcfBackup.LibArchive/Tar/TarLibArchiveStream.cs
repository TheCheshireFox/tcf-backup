using System.Runtime.InteropServices;
using TcfBackup.LibArchive.Options;

namespace TcfBackup.LibArchive.Tar;

public class TarStreamLibArchiveInitializer : ILibArchiveInitializer
{
    private readonly Stream _stream;

    private readonly ArchiveOpenCallback _onOpen;
    private readonly ArchiveWriteCallback _onWrite;
    private readonly ArchiveCloseCallback _onClose;
    private readonly ArchiveFreeCallback _onFree;

    private GCHandle _onOpenHandle;
    private GCHandle _onWriteHandle;
    private GCHandle _onCloseHandle;
    private GCHandle _onFreeHandle;
    
    public TarStreamLibArchiveInitializer(Stream stream)
    {
        _stream = stream;

        _onOpenHandle = GCHandle.Alloc(_onOpen = OnOpen);
        _onWriteHandle = GCHandle.Alloc(_onWrite = OnWrite);
        _onCloseHandle = GCHandle.Alloc(_onClose = OnClose);
        _onFreeHandle = GCHandle.Alloc(_onFree = OnFree);
    }

    private int OnOpen(nint archive, nint clientData)
    {
        return 0;
    }
    
    private unsafe long OnWrite(nint archive, nint clientData, nint buffer, long length)
    {
        var totalLength = length;

        try
        {
            while (length >= int.MaxValue)
            {
                _stream.Write(new Span<byte>(buffer.ToPointer(), int.MaxValue));

                buffer += int.MaxValue;
                length -= int.MaxValue;
            }

            _stream.Write(new Span<byte>(buffer.ToPointer(), (int)length));
        }
        catch (Exception e)
        {
            LibArchiveNativeWrapper.archive_set_error(archive, RetCode.Failed, e.Message);
            return -1;
        }
        
        return totalLength;
    }
    
    private int OnClose(nint archive, nint clientData)
    {
        try
        {
            _stream.Close();
        }
        catch (Exception e)
        {
            LibArchiveNativeWrapper.archive_set_error(archive, RetCode.Failed, e.Message);
            return -1;
        }
        
        return 0;
    }
    
    private int OnFree(nint archive, nint clientData)
    {
        return 0;
    }
    
    public void Initialize(nint archive)
    {
        LibArchiveNativeWrapper.archive_write_open2(archive, nint.Zero, _onOpen, _onWrite, _onClose, _onFree);
    }

    public void Cleanup(nint archive)
    {
        _onOpenHandle.Free();
        _onWriteHandle.Free();
        _onCloseHandle.Free();
        _onFreeHandle.Free();
    }
}

public class TarLibArchiveStream : TarLibArchiveBase
{
    public TarLibArchiveStream(Stream dst, TarOptions tarOptions, OptionsBase options)
        : base(new TarStreamLibArchiveInitializer(dst), tarOptions, options)
    {
        
    }
}