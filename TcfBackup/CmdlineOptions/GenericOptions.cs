using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace TcfBackup.CmdlineOptions;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class GenericOptions
{
    [Option('v', "verbose", Required = false, SetName = "Logging")]
    public bool Verbose { get; set; } = false;

    [Option('d', "debug", Required = false, SetName = "Logging")]
    public bool Debug { get; set; } = false;

#if DEBUG
    [Option("wait-debugger", Hidden = true, Required = false)]
    public bool WaitDebugger { get; set; }
#endif
}