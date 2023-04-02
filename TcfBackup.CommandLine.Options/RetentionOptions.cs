using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace TcfBackup.CommandLine.Options;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("retention", HelpText = "Restore managed backups")]
public class RetentionOptions : GenericOptions
{
    [Value(0, MetaName = "path", HelpText = "Configuration file", Required = true)]
    public IEnumerable<string> ConfigurationFiles { get; set; } = null!;
}