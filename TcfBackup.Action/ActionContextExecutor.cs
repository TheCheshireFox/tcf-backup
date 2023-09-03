using TcfBackup.Source;

namespace TcfBackup.Action;

public delegate Task ApplyFileListSourceAction(IFileListSource source, IActionContext context, CancellationToken cancellationToken);
public delegate Task ApplyStreamSourceAction(IStreamSource source, IActionContext context, CancellationToken cancellationToken);

public interface IActionContextExecutor
{
    public Task<bool> ExecAsync(IActionContext context, CancellationToken cancellationToken);
}

public class FileListSourceActionContextExecutor : IActionContextExecutor
{
    private readonly ApplyFileListSourceAction _action;

    public FileListSourceActionContextExecutor(ApplyFileListSourceAction action)
    {
        _action = action;
    }

    public async Task<bool> ExecAsync(IActionContext context, CancellationToken cancellationToken)
    {
        if (!context.TryGetFileListSource(out var fileListSource))
        {
            return false;
        }

        await _action(fileListSource, context, cancellationToken);
        return true;
    }
}

public class StreamSourceActionContextExecutor : IActionContextExecutor
{
    private readonly ApplyStreamSourceAction _action;

    public StreamSourceActionContextExecutor(ApplyStreamSourceAction action)
    {
        _action = action;
    }

    public async Task<bool> ExecAsync(IActionContext context, CancellationToken cancellationToken)
    {
        if (!context.TryGetStreamSource(out var streamSource))
        {
            return false;
        }

        await _action(streamSource, context, cancellationToken);
        return true;
    }
}

public class ActionContextExecutor
{
    private readonly IActionContext _actionContext;
    private readonly List<IActionContextExecutor> _actionExecutors = new();

    private ActionContextExecutor(IActionContext actionContext) => _actionContext = actionContext;

    private void AddAction(IActionContextExecutor actionExecutor)
    {
        if (_actionExecutors.Any(a => a.GetType() == actionExecutor.GetType()))
        {
            throw new ArgumentException($"Action with type {actionExecutor.GetType()} already exists");
        }
        
        _actionExecutors.Add(actionExecutor);
    }
    
    public static ActionContextExecutor For(IActionContext actionContext) => new(actionContext);

    public ActionContextExecutor ApplyFileListSource(ApplyFileListSourceAction action)
    {
        AddAction(new FileListSourceActionContextExecutor(action));
        return this;
    }
    
    public ActionContextExecutor ApplyStreamSource(ApplyStreamSourceAction action)
    {
        AddAction(new StreamSourceActionContextExecutor(action));
        return this;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var executor in _actionExecutors)
        {
            if (await executor.ExecAsync(_actionContext, cancellationToken))
            {
                return;
            }
        }

        throw new Exception("Unable to find any source");
    }
}