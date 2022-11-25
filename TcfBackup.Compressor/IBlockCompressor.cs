namespace TcfBackup.Compressor;

public interface IBlockCompressor : IDisposable
{
    public void Compress(Span<byte> buffer, Stream destination);
    public void Flush(Stream destination);
}