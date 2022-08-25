using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace TcfBackup.CmdlineOptions;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[Verb("backup", HelpText = "Perform backups")]
public class BackupOptions : GenericOptions
{
    [Value(0, MetaName = "path", HelpText = "Configuration file", Required = true)]
    public IEnumerable<string> ConfigurationFiles { get; set; } = null!;
}