using TcfBackup.Filesystem;
using TcfBackup.Source;

namespace TcfBackup.Target;

public class DirTarget : ITarget
{
    private readonly bool _overwrite;

    public string Scheme => TargetSchemes.Filesystem;
    public string Directory { get; }

    public DirTarget(IFilesystem filesystem, string dir, bool overwrite)
    {
        filesystem.CreateDirectory(Directory = dir);
        _overwrite = overwrite;
    }

    public void Apply(ISource source, CancellationToken cancellationToken)
    {
        foreach (var file in source.GetFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();
            file.Move(Path.Combine(Directory, Path.GetFileName(file.Path)), _overwrite);
        }
    }
}