using System;

namespace TcfBackup.Configuration.Source;

public class LxdSourceOptions : SourceOptions
{
    public string[] Containers { get; set; } = Array.Empty<string>();
    public bool IgnoreMissing { get; set; } = false;
}