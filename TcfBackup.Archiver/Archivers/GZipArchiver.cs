using TcfBackup.Compressor;

namespace TcfBackup.Archiver.Archivers;

public class GZipArchiver : CompressorArchiver
{
    public GZipArchiver(Stream output) : base(CompressorType.GZIP, output)
    {
    }
}