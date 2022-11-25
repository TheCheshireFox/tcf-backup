using System.IO.Abstractions;

namespace TcfBackup.Filesystem;

public class FileSystem : IFileSystem
{
    public FileSystem(System.IO.Abstractions.IFileSystem filesystem, string? tempDirectory = null)
    {
        DriveInfo = filesystem.DriveInfo;
        DirectoryInfo = filesystem.DirectoryInfo;
        FileInfo = filesystem.FileInfo;
        Path = !string.IsNullOrEmpty(tempDirectory)
            ? new SpecificTempDirectoryPathWrapper(filesystem, tempDirectory)
            : filesystem.Path;
        File = filesystem.File;
        Directory = new OptimizedDirectory(filesystem, filesystem.Directory);
        FileStream = filesystem.FileStream;
        FileSystemWatcher = filesystem.FileSystemWatcher;
    }
    
    public System.IO.Abstractions.IFile File { get; }
    public IOptimizedDirectory Directory { get; }
    public IFileInfoFactory FileInfo { get; }
    public IFileStreamFactory FileStream { get; }
    public IPath Path { get; }
    public IDirectoryInfoFactory DirectoryInfo { get; }
    public IDriveInfoFactory DriveInfo { get; }
    public IFileSystemWatcherFactory FileSystemWatcher { get; }

    public void Dispose()
    {
        if (Path is IDisposable disposablePath)
        {
            disposablePath.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
}