using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace TcfBackup.BtrfsUtil;

[StructLayout(LayoutKind.Sequential)]
internal struct TimeSpec
{
    public int UnixTime;
    public long NanoSec;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct BtrfsSubvolumeInfo
{
    public long Id;
    public long ParentId;
    public long DirId;
    public long Flags;
    public fixed byte UUID[16];
    public fixed byte ParentUUID[16];
    public fixed byte ReceivedUUID[16];
    public long Generation;
    public long CTransId;
    public long OTransId;
    public long STransId;
    public long RTransId;
    public TimeSpec CTime;
    public TimeSpec OTime;
    public TimeSpec STime;
    public TimeSpec RTime;
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum BtrfsUtilError
{
    OK,
    ERROR_STOP_ITERATION,
    ERROR_NO_MEMORY,
    ERROR_INVALID_ARGUMENT,
    ERROR_NOT_BTRFS,
    ERROR_NOT_SUBVOLUME,
    ERROR_SUBVOLUME_NOT_FOUND,
    ERROR_OPEN_FAILED,
    ERROR_RMDIR_FAILED,
    ERROR_UNLINK_FAILED,
    ERROR_STAT_FAILED,
    ERROR_STATFS_FAILED,
    ERROR_SEARCH_FAILED,
    ERROR_INO_LOOKUP_FAILED,
    ERROR_SUBVOL_GETFLAGS_FAILED,
    ERROR_SUBVOL_SETFLAGS_FAILED,
    ERROR_SUBVOL_CREATE_FAILED,
    ERROR_SNAP_CREATE_FAILED,
    ERROR_SNAP_DESTROY_FAILED,
    ERROR_DEFAULT_SUBVOL_FAILED,
    ERROR_SYNC_FAILED,
    ERROR_START_SYNC_FAILED,
    ERROR_WAIT_SYNC_FAILED,
    ERROR_GET_SUBVOL_INFO_FAILED,
    ERROR_GET_SUBVOL_ROOTREF_FAILED,
    ERROR_INO_LOOKUP_USER_FAILED,
    ERROR_FS_INFO_FAILED,
};

[Flags]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum CreateSnapshotFlags
{
    None = 0,
    Recursive = 1 << 0,
    ReadOnly = 1 << 1
}

[Flags]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal enum DeleteSnapshotFlags
{
    None = 0,
    Recursive = 1 << 0
}

internal static partial class BtrfsUtilNative
{
    [LibraryImport("btrfsutil", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial BtrfsUtilError btrfs_util_is_subvolume(string path);

    [LibraryImport("btrfsutil", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial BtrfsUtilError btrfs_util_subvolume_info(string path, long id, out BtrfsSubvolumeInfo subvolumeInfo);

    [LibraryImport("btrfsutil", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial BtrfsUtilError btrfs_util_create_snapshot(string source, string path, CreateSnapshotFlags flags, nint unused, nint qgroupInherit);

    [LibraryImport("btrfsutil", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new [] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial BtrfsUtilError btrfs_util_delete_subvolume(string path, DeleteSnapshotFlags flags);
}