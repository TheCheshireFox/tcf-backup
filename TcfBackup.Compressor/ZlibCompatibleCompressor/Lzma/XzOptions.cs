namespace TcfBackup.Compressor.ZlibCompatibleCompressor.Lzma;

public class XzOptions
{
    private readonly int _level = 6;
    
    public int Level
    {
        get => _level;
        init => _level = value is >= 0 and <= 9 ? value : throw new XzException($"Level should be in range [0..9], got: {value}");
    }

    public uint? Threads { get; init; }
    public ulong BlockSize { get; set; }
}