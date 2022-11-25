namespace TcfBackup.Archiver.Archivers.Tar;

internal static class StringExtensions
{
    public static string EnsureTrailingSlash(this string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
}