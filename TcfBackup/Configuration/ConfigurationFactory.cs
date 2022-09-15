using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TcfBackup.Shared;
using YamlDotNet.Serialization;

namespace TcfBackup.Configuration;

public static class ConfigurationFactory
{
    private static readonly IReadOnlyDictionary<string, Func<string, IConfiguration>> s_parsers =
        new Dictionary<string, Func<string, IConfiguration>>
        {
            { ".yaml", LoadYaml },
            { ".json", LoadJson }
        };

    private static readonly Func<string, IConfiguration> s_defaultParser = s_parsers[".yaml"];

    private static string FindConfigurationFile(string configurationFile)
    {
        if (!File.Exists(configurationFile))
        {
            configurationFile = Path.Combine(AppEnvironment.TcfConfigDirectory, configurationFile);
        }

        if (!File.Exists(configurationFile))
        {
            throw new FileNotFoundException($"Configuration file {configurationFile} not found.");
        }

        return configurationFile;
    }

    private static IConfiguration ReadConfiguration(string? configurationFile)
    {
        if (configurationFile == null)
        {
            throw new ArgumentException("No configuration file given", nameof(configurationFile));
        }

        configurationFile = FindConfigurationFile(configurationFile);
        var ext = PathUtils.GetFullExtension(configurationFile);

        if (s_parsers.TryGetValue(ext, out var parser))
        {
            return parser(configurationFile);
        }

        return string.IsNullOrEmpty(ext)
            ? s_defaultParser(configurationFile)
            : throw new NotSupportedException(ext);
    }

    private static IConfiguration LoadYaml(string configurationFile)
    {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        using var ms = new MemoryStream();
        ms.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deserializer.Deserialize(new StreamReader(configurationFile)))));
        ms.Seek(0, SeekOrigin.Begin);

        return new ConfigurationBuilder()
            .AddJsonStream(ms)
            .Build();
    }

    private static IConfiguration LoadJson(string configurationFile)
    {
        using var file = File.OpenRead(configurationFile);

        return new ConfigurationBuilder()
            .AddJsonStream(file)
            .Build();
    }

    public static IConfiguration CreateConfiguration(string? path) => ReadConfiguration(path);
}