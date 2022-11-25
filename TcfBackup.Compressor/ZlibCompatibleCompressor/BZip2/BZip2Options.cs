namespace TcfBackup.Compressor.ZlibCompatibleCompressor.BZip2;

public class BZip2Options
{
    private readonly int _blockSize = 9;
    private readonly int _verbosity = 0;
    private readonly int _workFactor = 0;

    public int BlockSize
    {
        get => _blockSize;
        init => _blockSize = value is >= 1 and <= 9 ? value : throw new BZip2Exception($"Block size should be in range [1..9], got: {value}");
    }
    
    public int Verbosity
    {
        get => _verbosity;
        init => _verbosity = value is >= 0 and <= 4 ? value : throw new BZip2Exception($"Verbosity should be in range [1..9], got: {value}");
    }
    
    public int WorkFactor
    {
        get => _workFactor;
        init => _workFactor = value is >= 0 and <= 250 ? value : throw new BZip2Exception($"Work factor should be in range [1..9], got: {value}");
    }
}