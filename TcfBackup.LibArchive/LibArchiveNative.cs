using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace TcfBackup.LibArchive;

public enum FilterCode
{
    None = 0,
    GZip = 1,
    BZip2 = 2,
    Compress = 3,
    Program = 4,
    Lzma = 5,
    Xz = 6,
    Uu = 7,
    Rpm = 8,
    LZip = 9,
    LrZip = 10,
    Lzop = 11,
    GrZip = 12,
    Lz4 = 13,
    Zstd = 14
}

public enum ArchiveFormat
{
    Cpio = 0x10000,
    Shar = 0x20000,
    Tar = 0x30000,
    TarPaxRestricted = Tar | 3,
    GnuTar = Tar | 4,
    Iso9660 = 0x40000,
    Zip = 0x50000,
    Empty = 0x60000,
    Ar = 0x70000,
    MTree = 0x80000,
    Raw = 0x90000,
    Xar = 0xA0000,
    Lha = 0xB0000,
    Cab = 0xC0000,
    Rar = 0xD0000,
    _7Zip = 0xE0000,
    Warc = 0xF0000,
    RarV5 = 0x100000
}

public enum RetCode
{
    Eof = 1,
    Ok = 0,
    Retry = -10,
    Warn = -20,
    Failed = -25,
    Fatal = -30
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum FileType : uint
{
    AE_IFMT = 0xF000,
    AE_IFREG = 0x8000,
    AE_IFLNK = 0xA000,
    AE_IFSOCK = 0xC000,
    AE_IFCHR = 0x2000,
    AE_IFBLK = 0x6000,
    AE_IFDIR = 0x4000,
    AE_IFIFO = 0x1000
}

public delegate int ArchiveOpenCallback(nint archive, nint clientData);
public delegate int ArchiveCloseCallback(nint archive, nint clientData);
public delegate int ArchiveFreeCallback(nint archive, nint clientData);
public delegate long ArchiveWriteCallback(nint archive, nint clientData, nint buffer, long length);

public static class LibArchiveNative
{
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint archive_write_new();
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint archive_read_disk_new();

    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void archive_entry_set_pathname(nint archiveEntry, ref byte pathBytes);

    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_write_add_filter(nint archive, FilterCode filterCode);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_write_set_format(nint archive, ArchiveFormat archiveFormat);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_write_open_filename(nint archive, string path);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int archive_write_open2(nint archive, nint clientData, ArchiveOpenCallback onOpen, ArchiveWriteCallback onWrite, ArchiveCloseCallback onClose, ArchiveFreeCallback onFree);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_read_disk_entry_from_file(nint archive, nint archiveEntry, int fd, nint stat);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint archive_entry_new();
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint archive_entry_clear(nint archiveEntry);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void archive_entry_free(nint archiveEntry);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_write_header(nint archive, nint archiveEntry);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long archive_write_data(nint archive, nint data, long len);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_write_close(nint archive);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_free(nint archive);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern RetCode archive_write_set_options(nint archive, string opts);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint archive_error_string(nint archive);

    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void archive_set_error(nint archive, RetCode err, ref byte message);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long archive_entry_size(nint archiveEntry);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int archive_entry_size_is_set(nint archiveEntry);
    
    [DllImport("libarchive.so.13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern FileType archive_entry_filetype(nint archiveEntry);
}