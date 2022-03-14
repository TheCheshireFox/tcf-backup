using TcfBackup.Filesystem;

namespace TcfBackup.Source
{
    public class TempDirectoryFileListSource : ISource
    {
        private readonly IFilesystem _fs;

        public string Directory { get; }

        public TempDirectoryFileListSource(IFilesystem fs, string dir)
        {
            _fs = fs;
            Directory = dir;
        }

        public IEnumerable<IFile> GetFiles() => _fs.GetFiles(Directory).Select(f => (IFile)new MutableFile(_fs, f));

        public void Prepare()
        {
        }

        public void Cleanup()
        {
            _fs.Delete(Directory);
        }
    }
}