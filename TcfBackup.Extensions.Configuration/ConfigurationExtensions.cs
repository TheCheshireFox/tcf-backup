using Microsoft.Extensions.Configuration;

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

    public static object Get(this IConfiguration configuration, Func<IConfiguration, Type> typeSelector, string key)
    {
        var cfg = configuration.GetSection(key);
        if (!cfg.Exists())
        {
            throw new KeyNotFoundException(key);
        }

        return cfg.Get(typeSelector);
    }

    public static object Get(this IConfiguration configuration, Func<IConfiguration, Type> typeSelector)
    {
        var type = typeSelector(configuration);
        if (type == null)
        {
            throw new KeyNotFoundException();
        }

        return configuration.Get(type);
    }

    public static IConfigurationSection GetSection(this IConfiguration configuration, string name, StringComparison nameComparison)
    {
        return configuration
                   .GetChildren()
                   .FirstOrDefault(c => string.Equals(c.Key, name, nameComparison))
               ?? (IConfigurationSection)new ConfigurationSection((IConfigurationRoot)configuration, name);
    }
}