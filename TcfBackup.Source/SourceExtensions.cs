using TcfBackup.Filesystem;

namespace TcfBackup.Source;

public static class SourceExtensions
{
    public static IEnumerable<IFile> GetFiles(this ISource source, bool followSymlinks)
    {
        return source is ISymlinkFilterable symlinkFilterableSource
            ? symlinkFilterableSource.GetFiles(followSymlinks)
            : source.GetFiles();
    }
}