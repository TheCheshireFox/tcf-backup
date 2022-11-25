using System.Runtime.CompilerServices;
using System.Text;

namespace TcfBackup.Archiver.Archivers.Tar;

internal unsafe class TarHeader
{
    private const int HeaderSize = 512;
    private const int ChecksumOffset = 148;

    public const int NameLength = 100;
    public const int LinkNameLength = 100;
    private const int ModeLength = 8;
    private const int UserIdLength = 8;
    private const int GroupIdLength = 8;
    private const int SizeLength = 12;
    private const int ModTimeLength = 12;
    private const int ChecksumLength = 8;
    private const int MagicLength = 8;
    private const int VersionLength = 6;
    private const int UserNameLength = 32;
    private const int GroupNameLength = 32;
    private const int DevMajorLength = 8;
    private const int DevMinorLength = 8;

    public string Name { get; init; } = string.Empty;
    public long Mode { get; init; }
    public long UserId { get; init; }
    public long GroupId { get; init; }
    public long Size { get; init; }
    public long ModTime { get; init; }
    public TypeFlag TypeFlag { get; init; }
    public string LinkName { get; init; } = string.Empty;
    public string Magic => "ustar  ";
    public short Version => 0;
    public string UserName { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
    public long DevMajor { get; init; }
    public long DevMinor { get; init; }

    private static byte* WriteString(string str, int maxCount, byte* dst, Encoding encoding)
    {
        if (encoding.GetByteCount(str) <= maxCount)
        {
            encoding.GetBytes(str, new Span<byte>(dst, maxCount));
        }
        return dst + maxCount;
    }

    private static byte* WriteOctal(long value, int digits, byte* dst)
    {
        if (value >=0 && value <= 1 << ((digits - 1) * 3))
        {
            var rDst = dst + digits - 1;
            *rDst-- = 0;
            while (value > 0 && rDst != dst)
            {
                *rDst-- = (byte)((byte)'0' + (byte)(value % 8));
                value /= 8;
            }
            
            if (rDst - dst > 0)
            {
                Unsafe.InitBlock(dst, (byte)'0', (uint)(rDst - dst + 1));
            }
        }
        else if (value >= -(1L << (8 * (digits - 1))) && value < 1L << (8 * (digits - 1)))
        {
            ulong ulValue;
            if (value > 0)
            {
                *dst = 0x80;
                ulValue = (ulong)value;
            }
            else
            {
                *dst = 0xFF;
                if (digits == 8)
                {
                    ulValue = 0UL - (ulong)-value;
                }
                else
                {
                    ulValue = (1UL << (8 * digits)) - (ulong)-value;
                }
            }
            
            var rDst = dst + digits - 1;
            while (ulValue > 0)
            {
                *rDst-- = (byte)(ulValue & 0xFF);
                ulValue >>= 8;
            }
        }

        return dst + digits;
    }

    private static byte* WriteByte(byte value, byte* dst)
    {
        *dst = value;
        return dst + 1;
    }

    private static byte* Fill(byte value, int count, byte* dst)
    {
        Unsafe.InitBlock(dst, value, (uint)count);
        return dst + count;
    }

    private static void CalcAndWriteChecksum(byte* buffer)
    {
        long checksum = 0;
        for (var i = 0; i < HeaderSize; i++)
        {
            checksum += buffer[i];
        }

        WriteOctal(checksum, ChecksumLength, buffer + ChecksumOffset);
    }
    
    public void Write(Stream stream, Encoding encoding)
    {
        var buffer = stackalloc byte[HeaderSize];

        var ptr = WriteString(Name, NameLength, buffer, encoding);
        ptr = WriteOctal(Mode, ModeLength, ptr);
        ptr = WriteOctal(UserId, UserIdLength, ptr);
        ptr = WriteOctal(GroupId, GroupIdLength, ptr);
        ptr = WriteOctal(Size, SizeLength, ptr);
        ptr = WriteOctal(ModTime, ModTimeLength, ptr);
        ptr = Fill((byte)' ', ChecksumLength, ptr); // Checksum
        ptr = WriteByte((byte)TypeFlag, ptr);
        ptr = WriteString(LinkName, LinkNameLength, ptr, encoding);
        ptr = WriteString(Magic, MagicLength, ptr, Encoding.ASCII);
        ptr = WriteOctal(Version, VersionLength, ptr);
        ptr = WriteString(UserName, UserNameLength, ptr, encoding);
        ptr = WriteString(GroupName, GroupNameLength, ptr, encoding);
        ptr = WriteOctal(DevMajor, DevMajorLength, ptr);
        WriteOctal(DevMinor, DevMinorLength, ptr);
        CalcAndWriteChecksum(buffer);
        
        stream.Write(new Span<byte>(buffer, HeaderSize));
    }
}