using System.IO.Enumeration;
using TcfBackup.Native;
using TcfBackup.Shared;

namespace TcfBackup.Filesystem;

public enum FilesystemNodeType
{
    Directory,
    File
}

public class FilesystemNode
{
    public string Name { get; }
    public string FullPath { get; }
    public FilesystemNodeType NodeType { get; }
    public FilesystemNode? Parent { get; }
    public IEnumerable<FilesystemNode> Children { get; }

    public FilesystemNode(FilesystemNode? parent, string name, FilesystemNodeType nodeType)
    {
        Name = name;
        NodeType = nodeType;
        FullPath = Path.Combine(parent == null ? Path.DirectorySeparatorChar.ToString() : parent.FullPath, name);
        Parent = parent;
        Children = GetChildren(FullPath);
    }

    private IEnumerable<FilesystemNode> GetChildren(string directory)
    {
        var opts = new EnumerationOptions
        {
            RecurseSubdirectories = false,
            AttributesToSkip = FileAttributes.Device,
            IgnoreInaccessible = true,
            ReturnSpecialDirectories = false
        };

        var entries = new FileSystemEnumerable<(string Name, bool IsDirectory)>(
            directory,
            (ref FileSystemEntry entry) => (entry.FileName.ToString(), entry.IsDirectory),
            opts);

        foreach (var entry in entries)
        {
            yield return new FilesystemNode(this, entry.Name, entry.IsDirectory ? FilesystemNodeType.Directory : FilesystemNodeType.File);
        }
    }
}

public class Filesystem : IFilesystem, IDisposable
{
    private static readonly string[] s_ignoreFsTypes = { "proc", "sysfs", "devtmpfs", "securityfs", "cgroup2", "efivarfs", "bpf" };

    private readonly string _tempDirectory;
    private readonly Action _tempDirectoryCleanup;

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

    public IEnumerable<string> GetFiles(string directory, bool throwIfPermissionDenied = false, bool followSymlinks = false)
    {
        // DriveInfo.GetDrives() returns wrong fs type for /dev
        var ignoreMountPointsRaw = File.ReadAllLines("/proc/mounts")
            .Select(l => l.Split(' ', 4, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(t => s_ignoreFsTypes.Contains(t[2]))
            .Select(t => t[1])
            .ToList();

        var ignoreMountPoints = ignoreMountPointsRaw
            .OrderByDescending(m => m.Length)
            .Where(m => !ignoreMountPointsRaw.Any(t => m.StartsWith(t) && t != m))
            .ToList();

        IEnumerable<string> EnumerateDirectories(string dir)
        {
            var containsExcludedDirs = false;
            var ignoredDirs = new List<string>();

            var subDirs = Directory.GetDirectories(dir, "*", new EnumerationOptions
            {
                RecurseSubdirectories = false,
                AttributesToSkip = FileAttributes.Device | FileAttributes.ReparsePoint,
                IgnoreInaccessible = !throwIfPermissionDenied
            });

            foreach (var subDir in subDirs)
            {
                if (ignoreMountPoints.Any(mp => mp.Equals(subDir)))
                {
                    containsExcludedDirs = true;
                    ignoredDirs.Add(subDir);
                    continue;
                }

                if (ignoreMountPoints.Any(mp => mp.StartsWith(subDir)))
                {
                    containsExcludedDirs = true;
                    ignoredDirs.Add(subDir);
                    foreach (var sDir in EnumerateDirectories(subDir))
                    {
                        yield return sDir;
                    }
                }
            }

            if (!containsExcludedDirs)
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

        return EnumerateDirectories(directory)
            .SelectMany(d => new FileSystemEnumerable<string>(d, (ref FileSystemEntry entry) => entry.ToFullPath(), new EnumerationOptions
            {
                RecurseSubdirectories = true,
                AttributesToSkip = FileAttributes.Device,
                IgnoreInaccessible = true,
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
            throw new IOException($"Path {path} not found");
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

    public Stream OpenRead(string path) => File.OpenRead(path);

    public Stream OpenWrite(string path) => File.OpenWrite(path);

    public void Dispose()
    {
        _tempDirectoryCleanup();
    }
}