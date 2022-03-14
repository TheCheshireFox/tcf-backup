using System.Collections.Generic;
using TcfBackup.Action;
using TcfBackup.Source;
using TcfBackup.Target;

namespace TcfBackup.Factory;

public interface IFactory
{
    ISource GetSource();
    IEnumerable<IAction> GetActions();
    ITarget GetTarget();
}