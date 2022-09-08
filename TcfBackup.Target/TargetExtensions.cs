using TcfBackup.Shared;

namespace TcfBackup.Target;

public static class TargetExtensions
{
    public static string GetTargetDirectory(this ITarget target) => $"{target.Scheme}://{PathUtils.AppendRoot(target.Directory)}";
}