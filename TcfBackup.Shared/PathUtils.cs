using System.Text.RegularExpressions;

namespace TcfBackup.Shared
{
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

        public static string GetCommonPath(string lhs, string rhs)
        {
            var lhsParts = lhs.Split(Path.DirectorySeparatorChar);
            var rhsParts = rhs.Split(Path.DirectorySeparatorChar);
            var result = new List<string>();

            for (var i = 0; i < Math.Min(lhsParts.Length, rhsParts.Length); i++)
            {
                if (lhsParts[i] == rhsParts[i])
                {
                    result.Add(lhsParts[i]);
                }
            }

            return string.Join(Path.DirectorySeparatorChar, result);
        }

        public static string GetRelativePath(string root, string path)
        {
            if (!path.StartsWith(root))
            {
                throw new IOException($"\"{path}\" is not part of \"{root}\"");
            }

            var relativePath = path[root.Length..];
            
            return relativePath.StartsWith("/") ? relativePath[1..] : relativePath;
        }
    }
}