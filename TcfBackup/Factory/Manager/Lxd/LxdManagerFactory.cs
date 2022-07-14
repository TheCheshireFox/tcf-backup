using System;
using TcfBackup.Managers;

namespace TcfBackup.Factory.Manager.Lxd;

public class LxdManagerFactoryScoped : ManagerFactoryScoped<ILxdManager, LxdManagerType>
{
    public ILxdManager Create(LxdManagerType selector) => selector switch
    {
        LxdManagerType.Executable => new LxdManager(),
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, null)
    };
}