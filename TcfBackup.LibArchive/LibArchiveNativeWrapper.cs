namespace TcfBackup.LibArchive;

public static class LibArchiveNativeWrapper
{
    private static unsafe RetCode ThrowIfError(nint archive, RetCode retCode, string message)
    {
        if (retCode == RetCode.Ok)
        {
            return retCode;
        }

        throw new LibArchiveException(retCode, $"[{retCode}]: {message}\n{new string((char*)LibArchiveNative.archive_error_string(archive).ToPointer())}");
    }

    private static RetCode ThrowIfError(RetCode retCode, string message)
    {
        if (retCode == RetCode.Ok)
        {
            return retCode;
        }

        throw new LibArchiveException(retCode, $"[{retCode}]: {message}");
    }
    
    private static nint ThrowIfError(nint ptr, string message)
    {
        if (ptr != nint.Zero)
        {
            return ptr;
        }

        throw new LibArchiveException(message);
    }

    public static nint archive_write_new()
        => ThrowIfError(LibArchiveNative.archive_write_new(), "Unable to create archive");
    public static nint archive_read_disk_new()
        => ThrowIfError(LibArchiveNative.archive_read_disk_new(), "Unable to create disk reader");
    public static void archive_entry_set_pathname(nint archiveEntry, ref byte pathBytes)
        => LibArchiveNative.archive_entry_set_pathname(archiveEntry, ref pathBytes);
    public static RetCode archive_write_add_filter(nint archive, FilterCode filterCode)
        => ThrowIfError(archive, LibArchiveNative.archive_write_add_filter(archive, filterCode), "Unable to add filter");
    public static RetCode archive_write_set_format(nint archive, ArchiveFormat archiveFormat)
        => ThrowIfError(archive, LibArchiveNative.archive_write_set_format(archive, archiveFormat), "string.Empty");
    public static RetCode archive_write_open_filename(nint archive, string path)
        => ThrowIfError(archive, LibArchiveNative.archive_write_open_filename(archive, path), "Unable to open archive for write");
    public static int archive_write_open2(nint archive, nint clientData, ArchiveOpenCallback onOpen, ArchiveWriteCallback onWrite, ArchiveCloseCallback onClose, ArchiveFreeCallback onFree)
        => LibArchiveNative.archive_write_open2(archive, clientData, onOpen, onWrite, onClose, onFree);
    public static RetCode archive_read_disk_entry_from_file(nint archive, nint archiveEntry, int fd, nint stat)
        => ThrowIfError(archive, LibArchiveNative.archive_read_disk_entry_from_file(archive, archiveEntry, fd, stat), "Unable to read file from disk");
    public static nint archive_entry_new()
        => ThrowIfError(LibArchiveNative.archive_entry_new(), "Unable to create file entry");
    public static nint archive_entry_clear(nint archiveEntry)
        => ThrowIfError(LibArchiveNative.archive_entry_clear(archiveEntry), "Unable to clear file entry");
    public static void archive_entry_free(nint archiveEntry)
        => LibArchiveNative.archive_entry_free(archiveEntry);
    public static RetCode archive_write_header(nint archive, nint archiveEntry)
        => ThrowIfError(archive, LibArchiveNative.archive_write_header(archive, archiveEntry), "Unable to write header");
    public static long archive_write_data(nint archive, nint data, long len)
        => LibArchiveNative.archive_write_data(archive, data, len);
    public static RetCode archive_write_close(nint archive)
        => ThrowIfError(archive, LibArchiveNative.archive_write_close(archive), "Unable to close data write stream");
    public static RetCode archive_free(nint archive)
        => ThrowIfError(archive, LibArchiveNative.archive_free(archive), "Unable to free archive memory");
    public static RetCode archive_write_set_options(nint archive, string opts)
        => ThrowIfError(archive, LibArchiveNative.archive_write_set_options(archive, opts), "Unable to set archive options");
    public static long archive_entry_size(nint archiveEntry)
        => LibArchiveNative.archive_entry_size(archiveEntry);
    public static int archive_entry_size_is_set(nint archiveEntry)
        => LibArchiveNative.archive_entry_size_is_set(archiveEntry);
    public static FileType archive_entry_filetype(nint archiveEntry)
        => LibArchiveNative.archive_entry_filetype(archiveEntry);
    public static void archive_set_error(nint archive, RetCode err, ref byte fmt)
        => LibArchiveNative.archive_set_error(archive, err, __arglist(ref fmt));
}
