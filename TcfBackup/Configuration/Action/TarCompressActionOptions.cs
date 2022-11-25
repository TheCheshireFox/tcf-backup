namespace TcfBackup.Configuration.Action;

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
    
}

public class XzOptions : ICompressorOptions
{
    public int Level { get; set; }
    public uint? Threads { get; init; }
    public ulong BlockSize { get; set; }
}

public enum GZipLevel
{
    NoCompression,
    Fastest,
    Optimal,
    Max
}

public class GZipOptions : ICompressorOptions
{
    public int Level { get; set; } = 6;
}