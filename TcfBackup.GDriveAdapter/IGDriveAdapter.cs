using System.IO;

namespace TcfBackup
{
    public interface IGDriveAdapter
    {
        void Authorize();
        string? CreateDirectory(string path);
        void UploadFile(Stream stream, string name, string? parentDirectoryId = null);
        void DownloadFile(string name, string destination, string? parentDirectoryId = null);
    }
}