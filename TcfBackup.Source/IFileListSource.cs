using TcfBackup.Filesystem;

namespace TcfBackup.Source;

public interface IFileListSource : ISource
{
    IEnumerable<IFile> GetFiles();
}