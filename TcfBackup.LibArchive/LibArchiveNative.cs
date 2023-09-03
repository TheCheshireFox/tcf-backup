using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace TcfBackup.LibArchive;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
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

[SuppressMessage("ReSharper", "UnusedMember.Global")]
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

[SuppressMessage("ReSharper", "UnusedMember.Global")]
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
[SuppressMessage("ReSharper", "UnusedMember.Global")]
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

internal static partial class LibArchiveNative
{
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial nint archive_write_new();
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial nint archive_read_disk_new();

    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void archive_entry_set_pathname(nint archiveEntry, ref byte pathBytes);

    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_write_add_filter(nint archive, FilterCode filterCode);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_write_set_format(nint archive, ArchiveFormat archiveFormat);
    
    [LibraryImport("libarchive.so.13", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_write_open_filename(nint archive, string path);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial int archive_write_open2(nint archive, nint clientData, ArchiveOpenCallback onOpen, ArchiveWriteCallback onWrite, ArchiveCloseCallback onClose, ArchiveFreeCallback onFree);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_read_disk_entry_from_file(nint archive, nint archiveEntry, int fd, nint stat);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial nint archive_entry_new();
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial nint archive_entry_clear(nint archiveEntry);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void archive_entry_free(nint archiveEntry);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_write_header(nint archive, nint archiveEntry);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long archive_write_data(nint archive, nint data, long len);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_write_close(nint archive);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_free(nint archive);
    
    [LibraryImport("libarchive.so.13", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial RetCode archive_write_set_options(nint archive, string opts);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial nint archive_error_string(nint archive);

    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void archive_set_error(nint archive, RetCode err, ref byte message);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long archive_entry_size(nint archiveEntry);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial int archive_entry_size_is_set(nint archiveEntry);
    
    [LibraryImport("libarchive.so.13")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial FileType archive_entry_filetype(nint archiveEntry);
}