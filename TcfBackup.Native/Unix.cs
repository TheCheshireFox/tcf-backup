using System.IO.Enumeration;
using System.Text;
using Mono.Unix;
using Mono.Unix.Native;

namespace TcfBackup.Native;

public static class Unix
{
    public enum FileType
    {
        File,
        Directory,
        Symlink
    }

    public struct FileOwner
    {
        public long UserId;
        public long GroupId;
        public string UserName;
        public string GroupName;
    }

    public struct FileInfo
    {
        public FileOwner Owner;
        public long Mode;
        public long DevMajor;
        public long DevMinor;
        public FileType FileType;
        public DateTime ModTime;
        public string? LinkTo;
    }

    public static FileInfo GetFileInfo(string path)
    {
        var unixFileInfo = new UnixFileInfo(path);

        var (devMajor, devMinor) = SplitDeviceNumber(unixFileInfo.Device);

        return new FileInfo
        {
            DevMajor = devMajor,
            DevMinor = devMajor,
            Mode = (long)unixFileInfo.FileAccessPermissions,
            ModTime = unixFileInfo.LastWriteTime,
            LinkTo = unixFileInfo.IsSymbolicLink ? new UnixSymbolicLinkInfo(path).ContentsPath : null,
            FileType = unixFileInfo.FileType switch
            {
                FileTypes.Directory => FileType.Directory,
                FileTypes.SymbolicLink => FileType.Symlink,
                _ => FileType.File
            },
            Owner = new FileOwner
            {
                UserId = unixFileInfo.OwnerUserId,
                GroupId = unixFileInfo.OwnerGroupId,
                UserName = unixFileInfo.OwnerUser.UserName,
                GroupName = unixFileInfo.OwnerGroup.GroupName
            }
        };
    }

    public static (long, long) SplitDeviceNumber(long deviceNumber)
    {
        var major = (((ulong)deviceNumber >> 32) & 0xfffff000) | (((ulong)deviceNumber >> 8) & 0xfff);
        var minor = (((ulong)deviceNumber >> 12) & 0xffffff00) | ((ulong)deviceNumber & 0xff);
        return ((long)major, (long)minor);
    }

    public static void CopyPermissions(string src, string dst)
    {
        UnixFileSystemInfo srcUnixFileInfo = Directory.Exists(src) ? new UnixDirectoryInfo(src) : new UnixFileInfo(src);
        UnixFileSystemInfo dstUnixFileInfo = Directory.Exists(dst) ? new UnixDirectoryInfo(dst) : new UnixFileInfo(dst);

        dstUnixFileInfo.FileAccessPermissions = srcUnixFileInfo.FileAccessPermissions;
        dstUnixFileInfo.FileSpecialAttributes = srcUnixFileInfo.FileSpecialAttributes;

        if (Syscall.llistxattr(src, Encoding.UTF8, out var names) < 0)
        {
            throw new Exception();
        }

        var xattrs = names.ToDictionary(name => name, name => Syscall.lgetxattr(src, name, out var value) < 0
            ? throw new Exception()
            : value);

        foreach (var (name, value) in xattrs)
        {
            if (Syscall.lsetxattr(src, name, value) < 0)
            {
                throw new Exception();
            }
        }
    }

    public static void CopyFile(string src, string dst, bool overwrite)
    {
        var dstExists = File.Exists(dst);

        var srcFileInfo = new System.IO.FileInfo(src);
        srcFileInfo.CopyTo(dst, overwrite);

        if (!dstExists)
        {
            CopyPermissions(src, dst);
        }
    }

    public static void CopyDirectory(string src, string dst, bool recursive)
    {
        static void CopyDirectoryInternal(string src, string dst, bool recursive)
        {
            if (!Directory.Exists(dst))
            {
                Directory.CreateDirectory(dst);
                CopyPermissions(src, dst);
            }

            foreach (var dir in Directory.EnumerateDirectories(src, "*", SearchOption.TopDirectoryOnly))
            {
                var dstDir = Path.Join(dst, dir[src.Length..]);

                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                    CopyPermissions(dir, dstDir);
                }

                foreach (var file in Directory.EnumerateFiles(dir))
                {
                    CopyFile(file, Path.Join(dstDir, Path.GetFileName(file)), true);
                }

                if (!recursive)
                {
                    continue;
                }

                foreach (var subDir in Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly))
                {
                    CopyDirectoryInternal(subDir, Path.Join(dst, subDir[src.Length..]), recursive);
                }
            }
        }

        CopyDirectoryInternal(src, dst, recursive);
    }

    public static void Move(string src, string dst, bool overwrite)
    {
        var srcFileInfo = new System.IO.FileInfo(src);
        srcFileInfo.MoveTo(dst, overwrite);

        CopyPermissions(src, dst);
    }
}