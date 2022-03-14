using TcfBackup.Filesystem;

namespace TcfBackup.Source;

public interface ISymlinkFilterable
{
    IEnumerable<IFile> GetFiles(bool followSymlinks);
}