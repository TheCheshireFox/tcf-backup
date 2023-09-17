namespace TcfBackup.Shared.ProgressLogger;

public class ProgressLogger : IProgressLogger
{
    private readonly long _threshold;

    private long _current;

    public event Action<long>? OnProgress;

    public static long GetThreshold(Stream stream)
    {
        const long kilobyte = 1024;
        
        try
        {
            return stream.Length switch
            {
                < 1 * 1024 * kilobyte => 100 * kilobyte,
                < 100 * 1024 * kilobyte => 1 * 1024 * kilobyte,
                _ => 10 * 1024 * kilobyte
            };
        }
        catch (Exception)
        {
            return 1024 * kilobyte;
        }
    }
    
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