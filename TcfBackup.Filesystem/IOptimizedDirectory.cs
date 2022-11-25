using System.IO.Abstractions;

namespace TcfBackup.Filesystem;

public interface IOptimizedDirectory : IDirectory
{
    IEnumerable<string> GetFiles(string path, bool recursive, bool sameFilesystem, bool skipAccessDenied, bool followSymlinks);
}