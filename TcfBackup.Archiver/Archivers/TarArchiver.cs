using System.Text;
using TcfBackup.Archiver.Archivers.Tar;
using TcfBackup.Filesystem;
using TcfBackup.Native;

namespace TcfBackup.Archiver.Archivers;

public delegate void OnEntryWritten(string path);

public class TarArchiver : IArchiver
{
    private const int BlockSize = 512;
    
    private readonly IFileSystem _fs;
    private readonly string _rootDir;
    private readonly bool _followSymlinks;
    private readonly HashSet<string> _archivedDirectories = new();

    private readonly Encoding _encoding = Encoding.UTF8;

    public event OnEntryWritten? OnEntryWritten;
    
    public Stream Output { get; }

    private static string NormalizePath(string path)
    {
        if (path.IndexOf('.') == -1)
        {
            return path;
        }
        
        var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var readyParts = new LinkedList<string>();

        foreach (var part in parts)
        {
            switch (part)
            {
                case ".":
                    continue;
                case ".." when readyParts.Count > 0:
                    readyParts.RemoveLast();
                    continue;
            }

            readyParts.AddLast(part);
        }

        if (Path.IsPathRooted(path))
        {
            readyParts.AddFirst(Path.DirectorySeparatorChar.ToString());
        }
        else if (path.StartsWith("./"))
        {
            readyParts.AddFirst(".");
        }
        
        return Path.Combine(readyParts.ToArray());
    }
    
    private TypeFlag GetTypeFlag(Unix.FileInfo fileInfo)
    {
        if (_followSymlinks && fileInfo.FileType == Unix.FileType.Symlink && fileInfo.LinkTo != null)
        {
            fileInfo = Unix.GetFileInfo(fileInfo.LinkTo);
        }

        return fileInfo.FileType switch
        {
            Unix.FileType.Block => TypeFlag.BlockDevice,
            Unix.FileType.Char => TypeFlag.CharacterDevice,
            Unix.FileType.Directory => TypeFlag.Directory,
            Unix.FileType.Fifo => TypeFlag.Fifo,
            Unix.FileType.File => TypeFlag.Regular,
            Unix.FileType.Symlink => _followSymlinks ? TypeFlag.Regular : TypeFlag.SymLink,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private bool IsFileReadable(Unix.FileInfo fileInfo) =>
        fileInfo.FileType == Unix.FileType.File || fileInfo.FileType == Unix.FileType.Symlink && _followSymlinks;

    private string TarifyPath(string path, bool isDirectory)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (isDirectory)
        {
            path = path.EnsureTrailingSlash();
        }
        
        if (path == _rootDir)
        {
            return "./";
        }
        
        return path.StartsWith(_rootDir)
            ? Path.Combine("./", path[_rootDir.Length..])
            : path;
    }

    private void TryArchiveDirectoryEntry(string path)
    {
        var parts = TarifyPath(path, true).Split(Path.DirectorySeparatorChar, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var part = parts.GetEnumerator();

        var currentPath = _rootDir;
        while (part.MoveNext())
        {
            currentPath = NormalizePath(Path.Join(currentPath, (string)part.Current!) + Path.DirectorySeparatorChar);
            if (_archivedDirectories.Add(currentPath))
            {
                WriteEntry(Unix.GetFileInfo(currentPath));
            }
        }
    }

    private void WriteLongHeaderIfNeeded(string name, int maxLen, TypeFlag type)
    {
        if (_encoding.GetByteCount(name) <= maxLen)
        {
            return;
        }
        
        var longHeader = new TarHeader
        {
            Name = "././@LongLink",
            TypeFlag = type,
            Size = _encoding.GetByteCount(name),
        };

        longHeader.Write(Output, _encoding);
        WritePayload(_encoding.GetBytes(name));
    }

    private void WritePayload(byte[] payload)
    {
        Output.Write(payload);
        WriteBlockSizeFill(payload.Length);
    }

    private void WriteBlockSizeFill(long size)
    {
        var remainder = size % BlockSize;
        if (remainder == 0)
        {
            return;
        }

        Span<byte> fill = stackalloc byte[BlockSize - (int)remainder];
        Output.Write(fill);
    }
    
    private void WriteEntry(Unix.FileInfo fileInfo)
    {
        var header = new TarHeader
        {
            Name = TarifyPath(fileInfo.Path, fileInfo.FileType == Unix.FileType.Directory),
            Mode = fileInfo.Mode,
            Size = IsFileReadable(fileInfo) ? fileInfo.Size : 0,
            DevMajor = fileInfo.DevMajor,
            DevMinor = fileInfo.DevMinor,
            GroupId = fileInfo.Owner.GroupId,
            GroupName = fileInfo.Owner.GroupName,
            UserId = fileInfo.Owner.UserId,
            UserName = fileInfo.Owner.UserName,
            LinkName = TarifyPath(_followSymlinks ? string.Empty : fileInfo.LinkTo ?? string.Empty, false),
            ModTime = new DateTimeOffset(fileInfo.ModTime).ToUnixTimeSeconds(),
            TypeFlag = GetTypeFlag(fileInfo)
        };

        WriteLongHeaderIfNeeded(header.LinkName, TarHeader.LinkNameLength, TypeFlag.LongLink);
        WriteLongHeaderIfNeeded(header.Name, TarHeader.NameLength, TypeFlag.LongName);
        header.Write(Output, _encoding);
        
        if (header.Size == 0)
        {
            return;
        }
        
        using var stream = _fs.File.Open(fileInfo.Path, FileMode.Open, FileAccess.Read);
        stream.CopyTo(Output, 1024 * 1024);

        WriteBlockSizeFill(header.Size);
        
        OnEntryWritten?.Invoke(header.Name);
    }

    public TarArchiver(IFileSystem fs, Stream output, string? rootDir = null, bool followSymlinks = false)
    {
        _fs = fs;
        _rootDir = (rootDir ?? Path.DirectorySeparatorChar.ToString()).EnsureTrailingSlash();
        _followSymlinks = followSymlinks;
        
        Output = output;
    }

    public void AddEntry(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new FormatException("Path should be absolute");
        }
        
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
        GC.SuppressFinalize(this);
    }
}