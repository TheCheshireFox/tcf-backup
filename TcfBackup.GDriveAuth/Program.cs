using Serilog;
using Serilog.Core;

namespace TcfBackup
{
    class Program
    {
        private const string LoggerFormat = "[{Level:u3}] [{SourceContext,-16}] {Message:lj}{NewLine}{Exception}";
        
        static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: LoggerFormat)
                .CreateLogger()
                .ForContext(Constants.SourceContextPropertyName, "Main");
            
            var gDriveAdapter = new GDriveAdapter(logger, new Filesystem.Filesystem());
            gDriveAdapter.Authorize();
        }
    }
}