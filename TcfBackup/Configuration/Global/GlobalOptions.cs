using System;
using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Global;

public enum LoggingParts
{
    Transfer
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class GlobalOptions
{
    public string Name { get; set; } = null!;
    public LoggingParts[] Logging { get; set; } = Array.Empty<LoggingParts>();
    public string? WorkingDir { get; set; }
    public RetentionOptions? Retention { get; set; }
}