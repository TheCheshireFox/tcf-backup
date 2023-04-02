using System.IO.Enumeration;

namespace TcfBackup.Filesystem;

public class FileSystemFile : IFileSystemFile
{
    public bool Exists(string path) => File.Exists(path);
    public void Copy(string source, string destination, bool overwrite) => File.Copy(source, destination, overwrite);

    public void Move(string source, string destination, bool overwrite) => File.Move(source, destination, overwrite);

    public void Delete(string path) => File.Delete(path);

    public Stream Open(string path, FileMode fileMode, FileAccess fileAccess) => File.Open(path, fileMode, fileAccess);

    public Stream Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) => File.Open(path, fileMode, fileAccess, fileShare);

    public Stream OpenRead(string path) => File.OpenRead(path);
}

public class FileSystemDirectory : IFileSystemDirectory
{
    private static IEnumerable<string> GetMountPoints()
    {
        return File.ReadLines("/proc/mounts")
            .Select(line => line.Split(' ', 3, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(t => t.Length >= 2)
            .Select(t => t[1]);
    }
    
    public bool Exists(string? path) => Directory.Exists(path);

    public void Create(string path) => Directory.CreateDirectory(path);

    public void Delete(string path, bool recursive = true) => Directory.Delete(path, recursive);

    public IEnumerable<string> GetFiles(string path, bool recursive = true, bool sameFilesystem = true, bool skipAccessDenied = false, bool followSymlinks = false)
    {
        var ignoreMountPoints = GetMountPoints()
            .OrderByDescending(m => m.Count(c => c == Path.DirectorySeparatorChar))
            .Where(m => m.StartsWith(path))
            .ToList();
        
        bool ShouldIgnoreMountPoint(ref FileSystemEntry fse)
        {
            if (!sameFilesystem)
            {
                return false;
            }
            
            var fsePath = fse.ToFullPath();
            return ignoreMountPoints.Any(mp => string.Equals(mp, fsePath) || fsePath.Contains(mp));
        }
        
        return new FileSystemEnumerable<string>(path, (ref FileSystemEntry entry) => entry.ToFullPath(), new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            AttributesToSkip = FileAttributes.Device,
            IgnoreInaccessible = skipAccessDenied,
            ReturnSpecialDirectories = false
        })
        {
            ShouldIncludePredicate = (ref FileSystemEntry fse) => !fse.IsDirectory,
            ShouldRecursePredicate = (ref FileSystemEntry fse) => recursive && !ShouldIgnoreMountPoint(ref fse) && (followSymlinks || !fse.Attributes.HasFlag(FileAttributes.ReparsePoint))
        };
    }
}

public class FileSystem : IFileSystem
{
    private readonly string? _tmpDirectory;

    public FileSystem(string? tmpDirectory)
    {
        _tmpDirectory = tmpDirectory;
    }

    public IFileSystemFile File { get; } = new FileSystemFile();
    public IFileSystemDirectory Directory { get; } = new FileSystemDirectory();
    public string GetTempPath() => _tmpDirectory ?? Path.GetTempPath();
    public string GetTempFileName()
    {
        if (_tmpDirectory == null)
        {
            return Path.GetTempFileName();
        }
        
        while (true)
        {
            var fileName = $"tmp{Random.Shared.Next():D10}.tmp";
            var path = Path.Combine(_tmpDirectory, fileName);
            try
            {
                using var file = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                return path;
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }
}