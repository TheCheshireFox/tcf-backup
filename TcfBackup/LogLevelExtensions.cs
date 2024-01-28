using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace TcfBackup;

public static class LogLevelExtensions
{
    public static LogEventLevel ToLogEventLevel(this LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical => LogEventLevel.Fatal,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Debug or LogLevel.Trace => LogEventLevel.Debug,
        _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
    };
}

public static class LogEventLevelExtensions
{
    public static LogLevel ToLogLevel(this LogEventLevel logEventLevel) => logEventLevel switch
    {
        LogEventLevel.Fatal => LogLevel.Critical,
        LogEventLevel.Error => LogLevel.Error,
        LogEventLevel.Warning => LogLevel.Warning,
        LogEventLevel.Information => LogLevel.Information,
        LogEventLevel.Verbose or LogEventLevel.Debug => LogLevel.Debug,
        _ => throw new ArgumentOutOfRangeException(nameof(logEventLevel), logEventLevel, null)
    };
}