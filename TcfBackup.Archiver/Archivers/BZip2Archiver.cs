using TcfBackup.Compressor;

namespace TcfBackup.Archiver.Archivers;

public class BZip2Archiver : CompressorArchiver
{
    public BZip2Archiver(Stream output) : base(CompressorType.BZIP2, output)
    {
    }
}