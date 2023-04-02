using System.Runtime.CompilerServices;
using System.Text;
using TcfBackup.LibArchive.Options;

namespace TcfBackup.LibArchive.Tar;

public abstract class TarLibArchiveBase : LibArchiveWriterBase
{
    private readonly TarOptions _tarOptions;
    
    private nint _readDisk;

    private string ChangeDir(string path)
    {
        if (string.IsNullOrEmpty(_tarOptions.ChangeDir) || !path.StartsWith(_tarOptions.ChangeDir))
        {
            return path;
        }

        path = path[_tarOptions.ChangeDir.Length..];
        path = path.StartsWith('/')
            ? $".{path}"
            : $"./{path}";

        return path;
    }

    private static void SetPathName(nint entry, string path)
    {
        Span<byte> pathBytes = stackalloc byte[4097];
        
        var count = Encoding.UTF8.GetBytes(path, pathBytes);
        Unsafe.InitBlock(ref Unsafe.AddByteOffset(ref pathBytes.GetPinnableReference(), count), 0, (uint)(pathBytes.Length - count));
        LibArchiveNativeWrapper.archive_entry_set_pathname(entry, ref pathBytes.GetPinnableReference());
    }
    
    protected TarLibArchiveBase(ILibArchiveInitializer initializer, TarOptions tarOptions, OptionsBase options)
        : base(initializer, ArchiveFormat.GnuTar, options)
    {
        _readDisk = LibArchiveNativeWrapper.archive_read_disk_new();
        _tarOptions = tarOptions;
    }

    protected override void SetupEntry(nint entry, string path)
    {
        SetPathName(entry, path);
        LibArchiveNativeWrapper.archive_read_disk_entry_from_file(_readDisk, entry, -1, nint.Zero);

        var relPath = ChangeDir(path);
        if (relPath != path)
        {
            SetPathName(entry, relPath);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_readDisk != nint.Zero)
        {
            try
            {
                LibArchiveNativeWrapper.archive_free(_readDisk);
            }
            catch (Exception)
            {
                // NOP
            }
            _readDisk = nint.Zero;
        }
        
        base.Dispose(disposing);
    }

    ~TarLibArchiveBase()
    {
        Dispose(false);
    }
}