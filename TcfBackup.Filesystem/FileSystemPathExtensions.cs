using System.IO.Abstractions;

namespace TcfBackup.Filesystem;

public static class FileSystemPathExtensions
{
    public static string GetTempFileName(this IPath path, string name, bool overwrite)
    {
        var filePath = path.Combine(path.GetTempPath(), name);
        using var file = overwrite
            ? path.FileSystem.File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
            : path.FileSystem.File.Open(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        return filePath;
    }

    public static string GetTempDirectoryName(this IPath path)
    {
        while (true)
        {
            var dirName = $"tmp{Random.Shared.Next():D10}.tmp";
            var dirPath = path.Combine(path.GetTempPath(), dirName);

            if (path.FileSystem.Directory.Exists(dirPath))
            {
                continue;
            }
            
            return path.FileSystem.Directory.CreateDirectory(dirPath).FullName;
        }
    }
}