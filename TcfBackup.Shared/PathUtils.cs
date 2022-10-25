using System.Text.RegularExpressions;

namespace TcfBackup.Shared;

public static class PathUtils
{
    // Order by priority
    private static readonly List<Regex> s_extRegexes = new()
    {
        new Regex(@".*(\.tar\.[^\.]+$)"),
        new Regex(@".+(\.[^\.]+$)")
    };

    public static string GetFullExtension(string path)
    {
        var match = s_extRegexes.Select(re => re.Match(path)).FirstOrDefault(m => m.Success);
        return match?.Success ?? false
            ? match.Groups[1].ToString()
            : string.Empty;
    }

    public static bool IsParentDirectory(string searchDir, string dir, bool allowSub = false)
    {
        var searchDirInfo = new DirectoryInfo(searchDir);
        
        if (!allowSub)
        {
            return new DirectoryInfo(dir).Parent?.FullName == searchDirInfo.FullName;
        }

        var dirDirInfo = new DirectoryInfo(dir);
        while (dirDirInfo.Parent != null)
        {
            if (dirDirInfo.Parent.FullName == searchDirInfo.FullName)
            {
                return true;
            }

            dirDirInfo = dirDirInfo.Parent;
        }

        return false;
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