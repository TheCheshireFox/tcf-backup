using System;
using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.Configuration.Action;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class FilterActionOptions : ActionOptions
{
    public bool FollowSymlinks { get; set; }
    public string[] Include { get; set; } = Array.Empty<string>();
    public string[] Exclude { get; set; } = Array.Empty<string>();
}