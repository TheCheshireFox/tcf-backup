using System.IO.Abstractions;

namespace TcfBackup.Filesystem;

internal class SpecificTempDirectoryPathWrapper : PathWrapper, IDisposable
{
    private readonly bool _tempDirectoryCreated;
    private readonly string _tempDirectory;
        
    public SpecificTempDirectoryPathWrapper(System.IO.Abstractions.IFileSystem fileSystem, string tempDirectory) : base(fileSystem)
    {
        if (fileSystem.Directory.Exists(_tempDirectory = tempDirectory))
        {
            return;
        }
        
        fileSystem.Directory.CreateDirectory(_tempDirectory);
        _tempDirectoryCreated = true;
    }

    public override string GetTempPath() => _tempDirectory;
        
    public override string GetTempFileName()
    {
        while (true)
        {
            var fileName = $"tmp{Random.Shared.Next():D10}.tmp";
            var path = Combine(_tempDirectory, fileName);
            try
            {
                using var file = FileSystem.File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                return path;
            }
            catch (Exception)
            {
                // NOP
            }
        }
    }

    public void Dispose()
    {
        if (!_tempDirectoryCreated)
        {
            return;
        }
        
        try
        {
            FileSystem.Directory.Delete(_tempDirectory, true);
        }
        catch (Exception)
        {
            // NOP
        }
        
        GC.SuppressFinalize(this);
    }
}