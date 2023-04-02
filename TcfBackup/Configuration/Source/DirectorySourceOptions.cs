using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Source;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class DirectorySourceOptions : SourceOptions
{
    public string Path { get; set; } = string.Empty;
}