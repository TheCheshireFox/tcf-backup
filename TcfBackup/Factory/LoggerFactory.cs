using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using TcfBackup.Shared;

namespace TcfBackup.Factory
{
    public class LoggerFactory : IServiceCollectionFactory<ILogger>
    {
        private readonly LoggerOptions _opts;
        
        public LoggerFactory(IOptions<LoggerOptions> opts)
        {
            _opts = opts.Value;
        }
        
        public ILogger Create() => new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: _opts.Format)
            .MinimumLevel.Is(_opts.LogLevel)
            .CreateLogger()
            .ForContext(Constants.SourceContextPropertyName, "Main");
    }
}