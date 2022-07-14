using System;
using TcfBackup.Managers;

namespace TcfBackup.Factory.Manager.Btrfs;

public class BtrfsManagerFactoryScoped : ManagerFactoryScoped<IBtrfsManager, BtrfsManagerType>
{
    public IBtrfsManager Create(BtrfsManagerType selector) => selector switch
    {
        BtrfsManagerType.Executable => new BtrfsManager(),
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, null)
    };
}