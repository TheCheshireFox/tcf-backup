using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Source;

namespace TcfBackup.Action;

public class DecryptAction : IAction
{
    private readonly IFilesystem _filesystem;
    private readonly IEncryptionManager _encryptionManager;

    public DecryptAction(IFilesystem filesystem, IEncryptionManager encryptionManager)
    {
        _filesystem = filesystem;
        _encryptionManager = encryptionManager;
    }

    public ISource Apply(ISource source)
    {
        var tmpDirSource = new TempDirectoryFileListSource(_filesystem, _filesystem.CreateTempDirectory());
        try
        {
            var decryptedFiles = source.GetFiles().ToDictionary(f => f, f => Path.Combine(tmpDirSource.Directory, Path.GetFileName(f.Path)));

            foreach (var (src, dst) in decryptedFiles)
            {
                _encryptionManager.Decrypt(src.Path, dst);
            }

            return tmpDirSource;
        }
        catch (Exception)
        {
            tmpDirSource.Cleanup();
            throw;
        }
    }
}