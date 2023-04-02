using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TcfBackup;

public interface IGDriveAdapter
{
    void Authorize();
    string? CreateDirectory(string path);
    Task UploadFileAsync(Stream stream, string name, string? parentDirectoryId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);
}