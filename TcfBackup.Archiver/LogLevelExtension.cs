namespace TcfBackup.Archiver;

public static class LogLevelExtension
{
    public static LogLevel ToLogLevel(this TcfBackup.LibArchive.LogLevel logLevel) => logLevel switch
    {
        LibArchive.LogLevel.Error => LogLevel.Error,
        LibArchive.LogLevel.Warning => LogLevel.Warning,
        _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
    };
}