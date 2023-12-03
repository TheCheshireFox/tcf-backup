using System.Runtime.InteropServices;

namespace TcfBackup.BtrfsUtil;

public class BtrfsException(string message, BtrfsUtilError? error = null)
    : Exception(error != null 
        ? $"{message} ({error})"
        : message)
{
    public BtrfsUtilError? Error { get; } = error;
}

public class SubvolumeInfo
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public long DirId { get; set; }
    public long Flags { get; set; }
    public Guid Uuid { get; set; }
    public Guid ParentUuid { get; set; }
    public Guid ReceivedUuid { get; set; }
    public long Generation { get; set; }
    public long ChangeTransId { get; set; }
    public long CreateTransId { get; set; }
    public long ReceiveSubvolumeTransId { get; set; }
    public long ReceiveTransId { get; set; }
    public DateTime ChangeTime { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime STime { get; set; }
    public DateTime ReceiveTime { get; set; }
}

public class BtrfsUtil
{
    public BtrfsUtil()
    {
        Marshal.PrelinkAll(typeof(BtrfsUtilNative));
    }
    
    public bool IsSubvolume(string path)
    {
        return BtrfsUtilNative.btrfs_util_is_subvolume(path) switch
        {
            BtrfsUtilError.OK => true,
            BtrfsUtilError.ERROR_NOT_BTRFS or BtrfsUtilError.ERROR_NOT_SUBVOLUME => false,
            var code => throw new BtrfsException($"Unable to check if {path} is subvolume", code)
        };
    }
    
    public void CreateSnapshot(string subvolume, string targetDir)
    {
        var ret = BtrfsUtilNative.btrfs_util_create_snapshot(subvolume, targetDir, CreateSnapshotFlags.ReadOnly, nint.Zero, nint.Zero);
        if (ret != BtrfsUtilError.OK)
        {
            throw new BtrfsException($"Unable to create snapshot", ret);
        }
    }
    
    public void DeleteSubvolume(string subvolume)
    {
        var ret = BtrfsUtilNative.btrfs_util_delete_subvolume(subvolume, DeleteSnapshotFlags.None);
        if (ret != BtrfsUtilError.OK)
        {
            throw new BtrfsException($"Unable to delete snapshot", ret);
        }
    }
    
    public SubvolumeInfo GetSubvolumeInfo(string path, long id = 0)
    {
        var ret = BtrfsUtilNative.btrfs_util_subvolume_info(path, id, out var info);
        if (ret != BtrfsUtilError.OK)
        {
            throw new BtrfsException($"Unable to get subvolume information for \"{path}\" with id {id}", ret);
        }

        unsafe
        {
            return new SubvolumeInfo
            {
                Id = info.Id,
                ParentId = info.ParentId,
                DirId = info.DirId,
                Flags = info.Flags,
                Uuid = new Guid(new ReadOnlySpan<byte>(info.UUID, 16)),
                ParentUuid = new Guid(new ReadOnlySpan<byte>(info.ParentUUID, 16)),
                ReceivedUuid = new Guid(new ReadOnlySpan<byte>(info.ReceivedUUID, 16)),
                Generation = info.Generation,
                ChangeTransId = info.CTransId,
                CreateTransId = info.OTransId,
                ReceiveSubvolumeTransId = info.STransId,
                ReceiveTransId = info.RTransId,
                ChangeTime = DateTimeOffset.FromUnixTimeSeconds(info.CTime.UnixTime).AddTicks(info.CTime.NanoSec / TimeSpan.NanosecondsPerTick).DateTime,
                CreateTime = DateTimeOffset.FromUnixTimeSeconds(info.OTime.UnixTime).AddTicks(info.OTime.NanoSec / TimeSpan.NanosecondsPerTick).DateTime,
                STime = new DateTime(),
                ReceiveTime = DateTimeOffset.FromUnixTimeSeconds(info.RTime.UnixTime).AddTicks(info.RTime.NanoSec / TimeSpan.NanosecondsPerTick).DateTime,
            };
        }
    }
}