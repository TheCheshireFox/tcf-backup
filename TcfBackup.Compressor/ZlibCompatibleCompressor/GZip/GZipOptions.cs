namespace TcfBackup.Compressor.ZlibCompatibleCompressor.GZip;

public class GZipOptions
{
    private readonly int _level = 6;
    
    public int Level
    {
        get => _level;
        init => _level = value is >= 0 and <= 9 ? value : throw new GZipException($"Level should be in range [0..9], got: {value}");
    }
}