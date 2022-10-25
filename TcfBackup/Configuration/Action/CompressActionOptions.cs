using LinqToDB.Common;

namespace TcfBackup.Configuration.Action;

public enum CompressAlgorithm
{
    None,
    BZip2,
    Xz,
    LZip,
    Lzma,
    Lzop,
    ZStd,
    Gzip
}

public class CompressActionOptions : ActionOptions
{
    public CompressAlgorithm[] Algorithms { get; set; } = Array<CompressAlgorithm>.Empty;
    public bool FollowSymlinks { get; set; }
    public string ChangeDir { get; set; } = string.Empty;
    public string? Name { get; set; }
}