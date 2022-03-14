using TcfBackup.Source;

namespace TcfBackup.Action
{
    public interface IAction
    {
        ISource Apply(ISource source);
    }
}