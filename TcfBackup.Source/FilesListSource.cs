using TcfBackup.Filesystem;

namespace TcfBackup.Source
{
    public class FilesListSource : ISource, IDisposable
    {
        private readonly IEnumerable<IFile> _files;

        public FilesListSource(IEnumerable<IFile> files) => _files = files;

        public static FilesListSource CreateMutable(IFilesystem fs, IEnumerable<string> files) => new(files.Select(f => (IFile)new MutableFile(fs, f)));
        public static FilesListSource CreateImmutable(IFilesystem fs, IEnumerable<string> files) => new(files.Select(f => (IFile)new ImmutableFile(fs, f)));

        public IEnumerable<IFile> GetFiles() => _files;

        public void Prepare()
        {
        }

        public void Cleanup()
        {
            foreach (var file in _files)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    // NOP
                }
            }
        }

        public void Dispose() => Cleanup();
    }
}