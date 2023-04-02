using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Action;

public class TarOptions
{
    public string? Name { get; set; }
    public string? ChangeDir { get; set; }
    public bool FollowSymlinks { get; set; }
}

public enum TarCompressor
{
    None,
    BZip2,
    Xz,
    Gzip
}

public interface ICompressorOptions
{
    
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class BZip2Options : ICompressorOptions
{
    public int Level { get; set; }
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class XzOptions : ICompressorOptions
{
    public int Level { get; set; }
    public uint? Threads { get; init; }
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class GZipOptions : ICompressorOptions
{
    public int Level { get; set; } = 6;
}