using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace TcfBackup.CmdlineOptions;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[Verb("restore", HelpText = "Restore managed backups")]
public class RestoreOptions : GenericOptions
{
    [Value(0, MetaName = "path", HelpText = "Configuration file", Required = true)]
    public string? ConfigurationFile { get; set; }
}