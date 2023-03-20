namespace TcfBackup.Configuration.Action;

public class TarOptions
{
    public string? Name { get; set; }
    public string? ChangeDir { get; set; }
    public bool FollowSymlinks { get; set; }
}

public enum TarCompressor
{
    BZip2,
    Xz,
    Gzip
}

public interface ICompressorOptions
{
    
}

public class BZip2Options : ICompressorOptions
{
    public int Level { get; set; }
}

public class XzOptions : ICompressorOptions
{
    public int Level { get; set; }
    public uint? Threads { get; init; }
}

public class GZipOptions : ICompressorOptions
{
    public int Level { get; set; } = 6;
}