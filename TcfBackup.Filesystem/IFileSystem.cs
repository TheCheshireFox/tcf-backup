using System.IO.Abstractions;

namespace TcfBackup.Filesystem;

public interface IFileSystem : IDisposable
{
    System.IO.Abstractions.IFile File { get; }
    IOptimizedDirectory Directory { get; }
    IFileInfoFactory FileInfo { get; }
    IFileStreamFactory FileStream { get; }
    IPath Path { get; }
    IDirectoryInfoFactory DirectoryInfo { get; }
    IDriveInfoFactory DriveInfo { get; }
    IFileSystemWatcherFactory FileSystemWatcher { get; }
}