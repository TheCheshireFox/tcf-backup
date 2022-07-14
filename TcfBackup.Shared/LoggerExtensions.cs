using Serilog;
using Serilog.Core;

namespace TcfBackup.Shared;

public static class LoggerExtensions
{
    private static Task LogReader(StreamReader reader, Action<string> log)
    {
        return Task.Factory.StartNew(() =>
        {
            while (reader.ReadLine() is { } line)
            {
                log(line);
            }
        }, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public static ProcessRedirects GetProcessRedirects(this ILogger logger)
    {
        return (input, output, error) => Task.WaitAll(LogReader(output, logger.Information), LogReader(error, logger.Error));
    }

    public static ILogger ForContextShort<T>(this ILogger logger) => logger.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);
}