using Serilog.Events;
using TcfBackup.CommandLine.Options;

namespace TcfBackup;

public class LoggerOptions
{
    public LogEventLevel LogLevel { get; private set; } = LogEventLevel.Information;
    public string Format => "[{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    public void Fill(GenericOptions opts)
    {
        LogLevel = opts switch
        {
            { Debug: true } => LogEventLevel.Debug,
            { Verbose: true } => LogEventLevel.Verbose,
            _ => LogEventLevel.Information
        };
    }
}