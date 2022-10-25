using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Tar;
using TcfBackup.Filesystem;
using TcfBackup.Native;

namespace TcfBackup.Archiver.Archivers;

public class TarArchiver : IFilesArchiver
{
    private readonly IFilesystem _fs;
    private readonly bool _followSymlinks;
    private readonly TarOutputStream _archiveOut;
    private readonly TarArchive _archive;
    private readonly HashSet<string> _archivedDirectories = new();

    public Stream Output { get; }

    private byte GetTypeFlag(Unix.FileInfo fileInfo)
    {
        if (_followSymlinks && fileInfo.FileType == Unix.FileType.Symlink && fileInfo.LinkTo != null)
        {
            fileInfo = Unix.GetFileInfo(fileInfo.LinkTo);
        }

        return fileInfo.FileType switch
        {
            Unix.FileType.Block => TarHeader.LF_BLK,
            Unix.FileType.Char => TarHeader.LF_CHR,
            Unix.FileType.Directory => TarHeader.LF_DIR,
            Unix.FileType.Fifo => TarHeader.LF_FIFO,
            Unix.FileType.File => TarHeader.LF_NORMAL,
            Unix.FileType.Symlink => _followSymlinks ? TarHeader.LF_NORMAL : TarHeader.LF_SYMLINK,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private bool IsFileReadable(Unix.FileInfo fileInfo) =>
        fileInfo.FileType == Unix.FileType.File || fileInfo.FileType == Unix.FileType.Symlink && _followSymlinks;

    private void TryArchiveDirectoryEntry(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new FormatException("Path should be absolute");
        }
        
        var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var part = parts.GetEnumerator();

        var currentPath = Path.DirectorySeparatorChar.ToString();
        while (part.MoveNext())
        {
            currentPath = Path.Join(currentPath, (string)part.Current!);
            if (_archivedDirectories.Add(currentPath))
            {
                WriteEntry(Unix.GetFileInfo(currentPath));
            }
        }
    }

    private void WriteEntry(Unix.FileInfo fileInfo)
    {
        var entry = TarEntry.CreateEntryFromFile(fileInfo.Path);
        entry.TarHeader.Mode = (int)fileInfo.Mode;
        entry.TarHeader.Size = IsFileReadable(fileInfo) ? fileInfo.Size : 0;
        entry.TarHeader.DevMajor = (int)fileInfo.DevMajor;
        entry.TarHeader.DevMinor = (int)fileInfo.DevMinor;
        entry.TarHeader.GroupId = (int)fileInfo.Owner.GroupId;
        entry.TarHeader.GroupName = fileInfo.Owner.GroupName;
        entry.TarHeader.UserId = (int)fileInfo.Owner.UserId;
        entry.TarHeader.UserName = fileInfo.Owner.UserName;
        entry.TarHeader.LinkName = _followSymlinks ? string.Empty : fileInfo.LinkTo ?? string.Empty;
        entry.TarHeader.TypeFlag = GetTypeFlag(fileInfo);

        _archiveOut.PutNextEntry(entry);
        if (entry.TarHeader.Size > 0)
        {
            using var stream = _fs.Open(fileInfo.Path, FileMode.Open, FileAccess.Read);
            stream.CopyTo(_archiveOut, 1024 * 1024);
        }
        _archiveOut.CloseEntry();
    }
    
    public TarArchiver(IFilesystem fs, Stream output, string? changeDir = null, bool followSymlinks = false)
    {
        _fs = fs;
        _followSymlinks = followSymlinks;
        _archiveOut = new TarOutputStream(output, Encoding.UTF8);
        _archive = TarArchive.CreateOutputTarArchive(_archiveOut);
        _archive.IsStreamOwner = false;

        if (changeDir != null)
        {
            _archive.RootPath = changeDir;
        }

        Output = output;
    }
    
    public void AddEntry(string path)
    {
        var fileInfo = Unix.GetFileInfo(path);
        if (fileInfo.FileType == Unix.FileType.Directory)
        {
            TryArchiveDirectoryEntry(path);
            return;
        }
        
        TryArchiveDirectoryEntry(Path.GetDirectoryName(path) ?? string.Empty);
        WriteEntry(fileInfo);
    }
    
    public void Dispose()
    {
        _archive.Dispose();
        GC.SuppressFinalize(this);
    }
}