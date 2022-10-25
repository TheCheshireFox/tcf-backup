using Serilog;
using Serilog.Core;

namespace TcfBackup.Shared;

public static class LoggerExtensions
{
    public static ILogger ForContextShort<T>(this ILogger logger) => logger.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);
}