using System;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using TcfBackup.Shared;

namespace TcfBackup.Factory;

public class LoggerFactory : IServiceCollectionFactory<ILogger>
{
    private class SimpleExceptionDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue? result)
        {
            if (value is not Exception exc)
            {
                result = null;
                return false;
            }
            
            result = propertyValueFactory.CreatePropertyValue(new { exc.Message }, destructureObjects: true);
            return true;
        }
    }
    
    private readonly LoggerOptions _opts;

    public LoggerFactory(IOptions<LoggerOptions> opts)
    {
        _opts = opts.Value;
    }

    public ILogger Create()
    {
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: _opts.Format)
            .MinimumLevel.Is(_opts.LogLevel);

        loggerConfiguration = _opts.LogLevel switch
        {
            LogEventLevel.Debug or LogEventLevel.Verbose => loggerConfiguration,
            _ => loggerConfiguration.Destructure.With(new SimpleExceptionDestructuringPolicy())
        };
        
        return loggerConfiguration
            .CreateLogger()
            .ForContext(Constants.SourceContextPropertyName, "Main");
    }
}