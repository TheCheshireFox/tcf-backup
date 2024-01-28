namespace TcfBackup.Configuration.Action.CompressAction;

public enum TarCompressor
{
    None,
    BZip2,
    Xz,
    Gzip
}

public class TarCompressActionOptions : CompressActionOptions
{
    public TarCompressor Compressor { get; set; }
    
    [DependsOn<BZip2Options>(nameof(Compressor), TarCompressor.BZip2)]
    [DependsOn<XzOptions>(nameof(Compressor), TarCompressor.Xz)]
    [DependsOn<GZipOptions>(nameof(Compressor), TarCompressor.Gzip)]
    public CompressorOptions Options { get; set; } = new();
}

public class CompressorOptions
{
}

public class BZip2Options : CompressorOptions
{
    public int Level { get; set; }
}

public class XzOptions : CompressorOptions
{
    public int Level { get; set; }
    public uint? Threads { get; set; }
}

public class GZipOptions : CompressorOptions
{
    public int Level { get; set; } = 6;
}