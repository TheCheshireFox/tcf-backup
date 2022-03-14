using TcfBackup.Filesystem;
using TcfBackup.Source;

namespace TcfBackup.Target;

public class DirTarget : ITarget
{
    private readonly string _dir;
    private readonly bool _overwrite;

    public DirTarget(IFilesystem filesystem, string dir, bool overwrite)
    {
        filesystem.CreateDirectory(_dir = dir);
        _overwrite = overwrite;
    }

    public void Apply(ISource source)
    {
        foreach (var file in source.GetFiles())
        {
            file.Move(Path.Combine(_dir, Path.GetFileName(file.Path)), _overwrite);
        }
    }
}