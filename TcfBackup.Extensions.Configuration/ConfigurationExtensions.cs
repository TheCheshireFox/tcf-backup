using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

[module: UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Already handled by Microsoft.Extensions.Configuration")]

namespace TcfBackup.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static IConfiguration Merge(this IConfiguration target, IConfiguration source)
    {
        return (IConfigurationRoot)new ConfigurationManager()
            .AddConfiguration(target)
            .AddConfiguration(source);
    }
    
    public static bool ContainsKey(this IConfiguration configuration, string name) => configuration.GetSection(name).Exists();

    public static IConfigurationSection GetSection(this IConfiguration configuration, string name, StringComparison nameComparison)
    {
        return configuration
                   .GetChildren()
                   .FirstOrDefault(c => string.Equals(c.Key, name, nameComparison))
               ?? new ConfigurationSection((IConfigurationRoot)configuration, name);
    }
}