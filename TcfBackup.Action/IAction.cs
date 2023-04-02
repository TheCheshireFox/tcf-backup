namespace TcfBackup.Action;

public interface IAction
{
    Task ApplyAsync(IActionContext actionContext, CancellationToken cancellationToken);
}