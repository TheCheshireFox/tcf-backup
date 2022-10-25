using TcfBackup.Compressor;

namespace TcfBackup.Archiver.Archivers;

public class XzArchiver : CompressorArchiver
{
    public XzArchiver(Stream output) : base(CompressorType.XZ, output)
    {
    }
}