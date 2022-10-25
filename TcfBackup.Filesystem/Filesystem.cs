using System.IO.Enumeration;
using TcfBackup.Native;
using TcfBackup.Shared;

namespace TcfBackup.Filesystem;

public class Filesystem : IFilesystem, IDisposable
{
    private readonly string _tempDirectory;
    private readonly Action _tempDirectoryCleanup;

    private static IEnumerable<string> EnumerateDirectories(string directory, bool throwIfPermissionDenied = false, bool oneFilesystem = false)
    {
        if (!oneFilesystem)
        {
            yield return directory;
            yield break;
        }
        
        // DriveInfo.GetDrives() returns wrong fs type for /dev
        var mountPoints = File.ReadAllLines("/proc/mounts")
            .Select(l => l.Split(' ', 4, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t[1])
            .OrderByDescending(m => m.Count(c => c == '/'))
            .ToList();
        
        var parentMountPoint = mountPoints.Contains(directory)
            ? directory
            : mountPoints
                .First(m => PathUtils.IsParentDirectory(m, directory, true));
        
        var ignoreMountPoints = mountPoints
            .Except(new[] { parentMountPoint })
            .Where(m => PathUtils.IsParentDirectory(parentMountPoint, m, true))
            .ToList();
        
        IEnumerable<string> EnumerateDirectoriesInternal(string dir)
        {
            var ignoredDirs = new List<string>();

            var subDirs = Directory.GetDirectories(dir, "*", new EnumerationOptions
            {
                RecurseSubdirectories = false,
                AttributesToSkip = FileAttributes.Device | FileAttributes.ReparsePoint,
                IgnoreInaccessible = !throwIfPermissionDenied
            });

            foreach (var subDir in subDirs)
            {
                if (ignoreMountPoints.Any(mp => string.Equals(mp, subDir) ||
                                                PathUtils.IsParentDirectory(mp, subDir, true)))
                {
                    ignoredDirs.Add(subDir);
                    continue;
                }
                
                foreach (var sDir in EnumerateDirectories(subDir))
                {
                    yield return sDir;
                }
            }

            if (ignoredDirs.Count == 0)
            {
                yield return dir;
            }
            else
            {
                foreach (var subDir in subDirs.Except(ignoredDirs))
                {
                    yield return subDir;
                }
            }
        }

        foreach (var dir in EnumerateDirectoriesInternal(directory))
        {
            yield return dir;
        }
    }

    public Filesystem(string? tempDirectory = null)
    {
        _tempDirectoryCleanup = () => { };

        if (tempDirectory != null)
        {
            _tempDirectory = tempDirectory;

            if (!DirectoryExists(tempDirectory))
            {
                _tempDirectoryCleanup = () => Delete(tempDirectory);
                CreateDirectory(tempDirectory);
            }
        }
        else
        {
            _tempDirectory = Path.GetTempPath();
        }
    }

    public IEnumerable<string> GetFiles(string directory, bool throwIfPermissionDenied = false, bool followSymlinks = false, bool oneFilesystem = false)
    {
        return EnumerateDirectories(directory, throwIfPermissionDenied, oneFilesystem)
            .SelectMany(d => new FileSystemEnumerable<string>(d, (ref FileSystemEntry entry) => entry.ToFullPath(), new EnumerationOptions
            {
                RecurseSubdirectories = true,
                AttributesToSkip = FileAttributes.Device,
                IgnoreInaccessible = !throwIfPermissionDenied,
                ReturnSpecialDirectories = false
            })
            {
                ShouldIncludePredicate = (ref FileSystemEntry fse) => !fse.IsDirectory || !followSymlinks && fse.Attributes.HasFlag(FileAttributes.ReparsePoint),
                ShouldRecursePredicate = (ref FileSystemEntry fse) => followSymlinks || !fse.Attributes.HasFlag(FileAttributes.ReparsePoint)
            });
    }

    public string CreateTempDirectory()
    {
        string tmpDir;
        while (Directory.Exists(tmpDir = Path.Combine(_tempDirectory, StringExtensions.GenerateRandomString(8))))
        {
        }

        return Directory.CreateDirectory(tmpDir).FullName;
    }

    public string CreateTempFile()
    {
        while (true)
        {
            var path = Path.Combine(_tempDirectory, StringExtensions.GenerateRandomString(8));
            try
            {
                File.Open(path, FileMode.Create).Dispose();
                return path;
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }

    public string CreateTempFile(string filename, bool replace = false)
    {
        using var f = File.Open(Path.Combine(_tempDirectory, filename), replace ? FileMode.OpenOrCreate : FileMode.Create);

        f.SetLength(0);

        return f.Name;
    }

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public bool FileExists(string? path) => File.Exists(path);

    public bool DirectoryExists(string? path) => Directory.Exists(path);

    public void Delete(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
            throw new FileNotFoundException($"Path {path} not found");
        }
    }

    public void Move(string source, string destination, bool overwrite)
    {
        if (string.IsNullOrEmpty(destination))
        {
            throw new ArgumentException(destination, nameof(destination));
        }

        if (Directory.Exists(source))
        {
            if (!overwrite && Directory.Exists(destination))
            {
                throw new IOException($"Directory {destination} already exists");
            }

            Directory.Move(source, destination);
        }
        else if (File.Exists(source))
        {
            Unix.Move(source, destination, overwrite);
        }
        else
        {
            throw new FileNotFoundException(source);
        }
    }

    public void CopyFile(string source, string destination, bool overwrite) => Unix.CopyFile(source, destination, overwrite);

    public void CopyDirectory(string source, string destination, bool recursive) => Unix.CopyDirectory(source, destination, recursive);

    public string[] ReadAllLines(string path) => File.ReadAllLines(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public void WriteAllLines(string path, IEnumerable<string> lines) => File.WriteAllLines(path, lines);

    public FileStream Open(string path, FileMode mode, FileAccess access) => File.Open(path, mode, access);

    public void Dispose()
    {
        _tempDirectoryCleanup();
        GC.SuppressFinalize(this);
    }
}