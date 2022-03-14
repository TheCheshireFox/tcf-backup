using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace TcfBackup.Factory;

public static class ConfigExtensions
{
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
}