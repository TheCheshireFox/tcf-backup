using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Source
{
    public class DirSource : ISource, ISymlinkFilterable
    {
        private readonly ILogger _logger;
        private readonly IFilesystem _filesystem;
        private readonly string _dir;

        public DirSource(ILogger logger, IFilesystem filesystem, string dir)
        {
            _logger = logger.ForContextShort<DirSource>();
            _filesystem = filesystem;
            
            if (!_filesystem.DirectoryExists(dir))
            {
                throw new DirectoryNotFoundException(dir);
            }

            _dir = dir;
        }

        public IEnumerable<IFile> GetFiles() => GetFiles(false);
        public IEnumerable<IFile> GetFiles(bool followSymlinks) => _filesystem.GetFiles(_dir, followSymlinks: followSymlinks).Select(f => (IFile)new ImmutableFile(_filesystem, f)).ToArray();

        public void Prepare()
        {
            _logger.Information("Prepared for listing files in directory {dir}", _dir);
        }

        public void Cleanup()
        {
            
        }
    }
}