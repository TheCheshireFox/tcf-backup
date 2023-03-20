using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace TcfBackup.Managers;

public class BtrfsManager : IBtrfsManager
{
    private static class BtrfsUtil
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
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
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public enum CreateSnapshotFlags
        {
            None = 0,
            Recursive = 1 << 0,
            ReadOnly = 1 << 1
        }
        
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public enum DeleteSnapshotFlags
        {
            None = 0,
            Recursive = 1 << 0
        }
            
        [DllImport("btrfsutil", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btrfs_util_create_snapshot")]
        public static extern BtrfsUtilError CreateSnapshot(string source, string path, CreateSnapshotFlags flags, nint unused, nint qgroupInherit);
            
        [DllImport("btrfsutil", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btrfs_util_delete_subvolume")]
        public static extern BtrfsUtilError DeleteSubvolume(string path, DeleteSnapshotFlags flags);
    }
    
    public void CheckAvailable()
    {
        Marshal.PrelinkAll(typeof(BtrfsUtil));
    }

    public void CreateSnapshot(string subvolume, string targetDir)
    {
        var ret = BtrfsUtil.CreateSnapshot(subvolume, targetDir, BtrfsUtil.CreateSnapshotFlags.ReadOnly, nint.Zero, nint.Zero);
        if (ret != BtrfsUtil.BtrfsUtilError.OK)
        {
            throw new Exception($"Unable to create snapshot: {ret}");
        }
    }

    public void DeleteSubvolume(string subvolume)
    {
        var ret = BtrfsUtil.DeleteSubvolume(subvolume, BtrfsUtil.DeleteSnapshotFlags.None);
        if (ret != BtrfsUtil.BtrfsUtilError.OK)
        {
            throw new Exception($"Unable to delete snapshot: {ret}");
        }
    }
}