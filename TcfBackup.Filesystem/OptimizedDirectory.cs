using System.IO.Abstractions;
using System.IO.Enumeration;

namespace TcfBackup.Filesystem;

public class OptimizedDirectory : IOptimizedDirectory
{
    private readonly IDirectory _directory;

    private static IEnumerable<string> GetMountPoints()
    {
        return File.ReadLines("/proc/mounts")
            .Select(line => line.Split(' ', 3, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(t => t.Length >= 2)
            .Select(t => t[1]);
    }

    public OptimizedDirectory(System.IO.Abstractions.IFileSystem fileSystem, IDirectory directory)
    {
        FileSystem = fileSystem;
        _directory = directory;
    }

    public IEnumerable<string> GetFiles(string path, bool recursive, bool sameFilesystem, bool skipAccessDenied, bool followSymlinks)
    {
        var ignoreMountPoints = GetMountPoints()
            .OrderByDescending(m => m.Count(c => c == FileSystem.Path.DirectorySeparatorChar))
            .Where(m => m.StartsWith(path))
            .ToList();
        
        bool ShouldIgnoreMountPoint(ref FileSystemEntry fse)
        {
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
    
    public IDirectoryInfo CreateDirectory(string path) => _directory.CreateDirectory(path);
    public IDirectoryInfo CreateDirectory(string path, UnixFileMode unixCreateMode) => _directory.CreateDirectory(path, unixCreateMode);

    public IFileSystemInfo CreateSymbolicLink(string path, string pathToTarget) => _directory.CreateSymbolicLink(path, pathToTarget);
    public IDirectoryInfo CreateTempSubdirectory(string? prefix = null)
    {
        throw new NotImplementedException();
    }

    public void Delete(string path) => _directory.Delete(path);

    public void Delete(string path, bool recursive) => _directory.Delete(path, recursive);

    public bool Exists(string path) => _directory.Exists(path);

    public DateTime GetCreationTime(string path) => _directory.GetCreationTime(path);

    public DateTime GetCreationTimeUtc(string path) => _directory.GetCreationTimeUtc(path);

    public string GetCurrentDirectory() => _directory.GetCurrentDirectory();

    public string[] GetDirectories(string path) => _directory.GetDirectories(path);

    public string[] GetDirectories(string path, string searchPattern) => _directory.GetDirectories(path, searchPattern);

    public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption) => _directory.GetDirectories(path, searchPattern, searchOption);

    public string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) => _directory.GetDirectories(path, searchPattern, enumerationOptions);

    public string GetDirectoryRoot(string path) => _directory.GetDirectoryRoot(path);

    public string[] GetFiles(string path) => _directory.GetFiles(path);

    public string[] GetFiles(string path, string searchPattern) => _directory.GetFiles(path, searchPattern);

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => _directory.GetFiles(path, searchPattern, searchOption);

    public string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) => _directory.GetFiles(path, searchPattern, enumerationOptions);

    public string[] GetFileSystemEntries(string path) => _directory.GetFileSystemEntries(path);

    public string[] GetFileSystemEntries(string path, string searchPattern) => _directory.GetFileSystemEntries(path, searchPattern);

    public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption) => _directory.GetFileSystemEntries(path, searchPattern, searchOption);
    public string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) => _directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);

    public DateTime GetLastAccessTime(string path) => _directory.GetLastAccessTime(path);

    public DateTime GetLastAccessTimeUtc(string path) => _directory.GetLastAccessTimeUtc(path);

    public DateTime GetLastWriteTime(string path) => _directory.GetLastWriteTime(path);

    public DateTime GetLastWriteTimeUtc(string path) => _directory.GetLastWriteTimeUtc(path);

    public string[] GetLogicalDrives() => _directory.GetLogicalDrives();

    public IDirectoryInfo GetParent(string path) => _directory.GetParent(path);

    public void Move(string sourceDirName, string destDirName) => _directory.Move(sourceDirName, destDirName);
    public IFileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget) => _directory.ResolveLinkTarget(linkPath, returnFinalTarget);

    public void SetCreationTime(string path, DateTime creationTime) => _directory.SetCreationTime(path, creationTime);

    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc) => _directory.SetCreationTimeUtc(path, creationTimeUtc);

    public void SetCurrentDirectory(string path) => _directory.SetCurrentDirectory(path);

    public void SetLastAccessTime(string path, DateTime lastAccessTime) => _directory.SetLastAccessTime(path, lastAccessTime);

    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc) => _directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

    public void SetLastWriteTime(string path, DateTime lastWriteTime) => _directory.SetLastWriteTime(path, lastWriteTime);

    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) => _directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

    public IEnumerable<string> EnumerateDirectories(string path) => _directory.EnumerateDirectories(path);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern) => _directory.EnumerateDirectories(path, searchPattern);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) => _directory.EnumerateDirectories(path, searchPattern, searchOption);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) => _directory.EnumerateDirectories(path, searchPattern, enumerationOptions);

    public IEnumerable<string> EnumerateFiles(string path) => _directory.EnumerateFiles(path);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern) => _directory.EnumerateFiles(path, searchPattern);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => _directory.EnumerateFiles(path, searchPattern, searchOption);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) => _directory.EnumerateFiles(path, searchPattern, enumerationOptions);

    public IEnumerable<string> EnumerateFileSystemEntries(string path) => _directory.EnumerateFileSystemEntries(path);

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern) => _directory.EnumerateFileSystemEntries(path, searchPattern);

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption) => _directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) => _directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);

    public System.IO.Abstractions.IFileSystem FileSystem { get; }
}