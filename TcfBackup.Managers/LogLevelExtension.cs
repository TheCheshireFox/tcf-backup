using Serilog.Events;
using TcfBackup.Archiver;

namespace TcfBackup.Managers;

public static class LogLevelExtension
{
    public static LogEventLevel ToLogEventLevel(this LogLevel archiverLogLevel) => archiverLogLevel switch
    {
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Warning => LogEventLevel.Warning,
        _ => throw new ArgumentOutOfRangeException(nameof(archiverLogLevel), archiverLogLevel, null)
    };
}