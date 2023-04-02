namespace TcfBackup.Shared;

public class ProgressLogger
{
    private readonly long _threshold;

    private long _current;

    public event Action<long>? OnProgress;

    public ProgressLogger(long threshold)
    {
        _threshold = threshold;
    }

    public void Set(long value)
    {
        var currentLevel = _current / _threshold;
        var newLevel = (_current = value) / _threshold;

        if (newLevel > currentLevel)
        {
            OnProgress?.Invoke(_current);
        }
    }
    
    public void Add(long value)
    {
        Set(_current + value);
    }
}