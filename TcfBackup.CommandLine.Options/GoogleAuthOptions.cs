using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace TcfBackup.CommandLine.Options;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("google-auth", HelpText = "Perform google authentication")]
public class GoogleAuthOptions : GenericOptions
{
}