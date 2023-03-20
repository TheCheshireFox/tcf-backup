namespace TcfBackup.Action;

public interface IAction
{
    void Apply(IActionContext actionContext, CancellationToken cancellationToken);
}