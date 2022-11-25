namespace TcfBackup.Archiver.Archivers.Tar;

internal enum TypeFlag : byte
{
    Regular = (byte)'0',
    Link = (byte)'1',
    SymLink = (byte)'2',
    CharacterDevice = (byte)'3',
    BlockDevice = (byte)'4',
    Directory = (byte)'5',
    Fifo = (byte)'6',
    ContiguousFile = (byte)'7',
    ExtendedHeader = (byte)'x',
    GlobalExtendedHeader = (byte)'g',
    LongName = (byte)'L',
    LongLink = (byte)'K',
    SparseFile = (byte)'S',
}