using TcfBackup.Filesystem;

namespace TcfBackup.Source
{
    public interface ISource
    {
        IEnumerable<IFile> GetFiles();
        void Prepare();
        void Cleanup();
    }
}