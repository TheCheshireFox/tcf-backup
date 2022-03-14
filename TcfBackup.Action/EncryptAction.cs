using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Shared;
using TcfBackup.Source;

namespace TcfBackup.Action
{
    public class EncryptAction : IAction
    {
        private readonly ILogger _logger;
        private readonly IFilesystem _filesystem;
        private readonly IEncryptionManager _encryptionManager;

        public EncryptAction(ILogger logger, IFilesystem filesystem, IEncryptionManager encryptionManager)
        {
            _logger = logger.ForContextShort<EncryptAction>();
            _filesystem = filesystem;
            _encryptionManager = encryptionManager;
        }

        public ISource Apply(ISource source)
        {
            _logger.Information("Start encryption");

            var targetDir = _filesystem.CreateTempDirectory();

            try
            {
                var encryptedFiles = new Dictionary<string, string>();
                foreach (var file in source.GetFiles())
                {
                    var dst = Path.Combine(targetDir, Path.GetFileName(file.Path));
                    while (_filesystem.FileExists(dst))
                    {
                        dst = Path.Combine(targetDir, StringExtensions.GenerateRandomString(8) + PathUtils.GetFullExtension(file.Path));
                    }

                    encryptedFiles.Add(file.Path, dst);
                }

                foreach (var (src, dst) in encryptedFiles)
                {
                    _logger.Information("Encrypting file {src} to {dst}...", src, dst);
                    _encryptionManager.Encrypt(src, dst);
                }

                _logger.Information("Encryption complete");

                return FilesListSource.CreateMutable(_filesystem, encryptedFiles.Values);
            }
            catch (Exception)
            {
                _filesystem.Delete(targetDir);
                throw;
            }
        }
    }
}