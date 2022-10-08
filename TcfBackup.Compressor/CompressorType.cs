using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Compressor;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum CompressorType
{
    GZIP = 0,
    BZIP2,
    XZ
}