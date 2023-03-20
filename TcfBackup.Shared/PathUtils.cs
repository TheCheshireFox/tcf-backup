using System.Text.RegularExpressions;

namespace TcfBackup.Shared;

public static partial class PathUtils
{
    [GeneratedRegex(".*(\\.tar\\.[^\\.]+$)")]
    private static partial Regex TarExtRe();
    
    [GeneratedRegex(@".+(\.[^\.]+$)")]
    private static partial Regex AllExtRe();
    
    // Order by priority
    private static readonly List<Regex> s_extRegexes = new()
    {
        TarExtRe(),
        AllExtRe()
    };

    public static string GetFullExtension(string path)
    {
        var match = s_extRegexes.Select(re => re.Match(path)).FirstOrDefault(m => m.Success);
        return match?.Success ?? false
            ? match.Groups[1].ToString()
            : string.Empty;
    }

    public static string? GetFileNameWithoutExtension(string? path)
    {
        if (path == null)
        {
            return null;
        }

        var ext = GetFullExtension(path);
        return path[..^ext.Length];
    }

    public static string AppendRoot(string path) =>
        Path.IsPathRooted(path)
            ? path
            : Path.Combine(Path.DirectorySeparatorChar.ToString(), path);
}