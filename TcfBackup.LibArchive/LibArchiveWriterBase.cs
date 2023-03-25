using TcfBackup.LibArchive.Options;

namespace TcfBackup.LibArchive;

public interface ILibArchiveInitializer
{
    void Initialize(nint archive);
    void Cleanup(nint archive);
}

public abstract class LibArchiveWriterBase : IDisposable
{
    private const int WriteBufferSize = 1024 * 1024;
    
    private readonly ILibArchiveInitializer _initializer;
    private readonly byte[] _buffer = new byte[WriteBufferSize];
    
    private nint _archive;
    private nint _entry;

    private void WriteEntryHeader()
    {
        try
        {
            LibArchiveNativeWrapper.archive_write_header(_archive, _entry);
        }
        catch (LibArchiveException exc)
        {
            if (exc.RetCode is not RetCode.Ok and not RetCode.Warn)
            {
                throw;
            }
            // It's fine, warnings are ok here
        }
    }

    protected LibArchiveWriterBase(ILibArchiveInitializer initializer, ArchiveFormat archiveFormat, OptionsBase options)
    {
        _initializer = initializer;
        _archive = LibArchiveNative.archive_write_new();
        LibArchiveNativeWrapper.archive_write_add_filter(_archive, options.FilterCode);
        LibArchiveNativeWrapper.archive_write_set_format(_archive, archiveFormat);
        LibArchiveNativeWrapper.archive_write_set_options(_archive, options.ToString());
        
        _initializer.Initialize(_archive);
        _entry = LibArchiveNative.archive_entry_new();
    }

    protected abstract bool SetupEntry(nint entry, string path);
    
    public unsafe void AddFile(string path)
    {
        try
        {
            if (!SetupEntry(_entry, path))
            {
                WriteEntryHeader();
                LibArchiveNativeWrapper.archive_entry_clear(_entry);
                return;
            }
            
            WriteEntryHeader();

            fixed (byte* buffer = _buffer)
            {
                var bufferSpan = new Span<byte>(buffer, _buffer.Length);
                
                using var stream = File.OpenRead(path);
                int read;
                while ((read = stream.Read(bufferSpan)) > 0)
                {
                    LibArchiveNativeWrapper.archive_write_data(_archive, (nint)buffer, read);
                }
            }

            LibArchiveNativeWrapper.archive_entry_clear(_entry);
        }
        catch (Exception exc)
        {
            throw new LibArchiveException($"Unable to add file {path}", exc);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_archive != nint.Zero)
        {
            try
            {
                _initializer.Cleanup(_archive);
            }
            catch (Exception)
            {
                // NOP
            }
        }
        
        if (_entry != nint.Zero)
        {
            try
            {
                LibArchiveNativeWrapper.archive_entry_free(_entry);
            }
            catch (Exception)
            {
                // NOP
            }
            _entry = nint.Zero;
        }
        
        if (_archive != nint.Zero)
        {
            try
            {
                LibArchiveNativeWrapper.archive_write_close(_archive);
                LibArchiveNativeWrapper.archive_free(_archive);
            }
            catch (Exception)
            {
                // NOP
            }
            _archive = nint.Zero;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~LibArchiveWriterBase() => Dispose(false);
}