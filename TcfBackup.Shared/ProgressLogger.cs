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

    public void Update(long value)
    {
        var currentLevel = _current / _threshold;
        var newLevel = (_current += value) / _threshold;

        if (newLevel > currentLevel)
        {
            OnProgress?.Invoke(_current);
        }
    }
}