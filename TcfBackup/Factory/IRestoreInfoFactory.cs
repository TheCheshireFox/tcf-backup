using System.Collections.Generic;
using TcfBackup.Restore;

namespace TcfBackup.Factory;

public interface IRestoreInfoFactory
{
    IRestoreSourceInfo GetRestoreSourceInfo();
    IEnumerable<IRestoreActionInfo> GetRestoreActionInfo();
    IRestoreTargetInfo GetRestoreTargetInfo();
}