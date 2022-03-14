using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Source;

namespace TcfBackup.Action
{
    public class DecompressAction : IAction
    {
        private readonly IFilesystem _filesystem;
        private readonly ICompressionManager _compressionManager;
        
        public DecompressAction(ICompressionManager compressionManager, IFilesystem filesystem)
        {
            _compressionManager = compressionManager;
            _filesystem = filesystem;
        }
        
        public ISource Apply(ISource source)
        {
            var tmpDirSource = new TempDirectoryFileListSource(_filesystem, _filesystem.CreateTempDirectory());
            try
            {
                foreach (var file in source.GetFiles())
                {
                    _ = _compressionManager.Decompress(file.Path, tmpDirSource.Directory);
                }
            }
            catch
            {
                tmpDirSource.Cleanup();
                throw;
            }

            return tmpDirSource;
        }
    }
}