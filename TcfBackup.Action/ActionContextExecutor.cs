using TcfBackup.Source;

namespace TcfBackup.Action;

public class ActionContextExecutor
{
    private enum ActionVisitorApplyType
    {
        FileListSource,
        StreamSource
    }
    
    private readonly IActionContext _actionContext;
    private readonly List<(ActionVisitorApplyType ApplyType, Delegate Action)> _actions = new();

    private ActionContextExecutor(IActionContext actionContext) => _actionContext = actionContext;

    private void AddAction(ActionVisitorApplyType applyType, Delegate action)
    {
        if (_actions.Any(a => a.ApplyType == applyType))
        {
            throw new ArgumentException($"Action with type {applyType} already exists");
        }
        
        _actions.Add((applyType, action));
    }
    
    public static ActionContextExecutor For(IActionContext actionContext) => new(actionContext);

    public ActionContextExecutor ApplyFileListSource(Action<IFileListSource, IActionContext, CancellationToken> action)
    {
        AddAction(ActionVisitorApplyType.FileListSource, action);
        return this;
    }
    
    public ActionContextExecutor ApplyStreamSource(Action<IStreamSource, IActionContext, CancellationToken> action)
    {
        AddAction(ActionVisitorApplyType.StreamSource, action);
        return this;
    }

    public void Execute(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _actions.Count; i++)
        {
            void ThrowIfLast()
            {
                if (i == _actions.Count - 1)
                {
                    throw new Exception("Unable to find any source");
                }
            }
            
            var (applyType, action) = _actions[i];
            
            switch (applyType)
            {
                case ActionVisitorApplyType.StreamSource:
                    if (!_actionContext.TryGetStreamSource(out var streamSource))
                    {
                        ThrowIfLast();
                        continue;
                    }
                    
                    ((Action<IStreamSource, IActionContext, CancellationToken>)action)(streamSource, _actionContext, cancellationToken);
                    return;
                case ActionVisitorApplyType.FileListSource:
                    if (!_actionContext.TryGetFileListSource(out var fileListSource))
                    {
                        ThrowIfLast();
                        continue;
                    }
                    
                    ((Action<IFileListSource, IActionContext, CancellationToken>)action)(fileListSource, _actionContext, cancellationToken);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(applyType), $"Unknown action type {applyType}");
            }
        }
    }
}