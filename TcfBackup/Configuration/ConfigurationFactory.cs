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
            { "", LoadYaml }, // default parser
            { ".yaml", LoadYaml },
            { ".json", LoadJson }
        };

    private static readonly string[] s_configurationSearchPaths = ["./", AppEnvironment.TcfConfigDirectory];

    private static bool TryFindConfigurationFileWithSupportedExtensions(string path, out string result)
    {
        if (!string.IsNullOrEmpty(PathUtils.GetFullExtension(path)))
        {
            if (File.Exists(path))
            {
                result = path;
                return true;
            }

            result = string.Empty;
            return false;
        }

        foreach (var ext in s_parsers.Keys.Where(k => !string.IsNullOrEmpty(k)))
        {
            if (File.Exists(result = path + ext))
            {
                return true;
            }
        }
        
        result = string.Empty;
        return false;
    }
    
    private static string FindConfigurationFile(string configurationFile)
    {
        FileNotFoundException CreateException() => new($"Configuration file {configurationFile} not found.");

        if (Path.IsPathRooted(configurationFile))
        {
            return TryFindConfigurationFileWithSupportedExtensions(configurationFile, out configurationFile)
                ? configurationFile
                : throw CreateException();
        }

        foreach (var searchPath in s_configurationSearchPaths)
        {
            var path = Path.GetFullPath(Path.Join(searchPath, configurationFile));

            if (TryFindConfigurationFileWithSupportedExtensions(path, out path))
            {
                return path;
            }
        }

        throw CreateException();
    }

    private static IConfiguration ReadConfiguration(string? configurationFile)
    {
        if (configurationFile == null)
        {
            throw new ArgumentException("No configuration file given", nameof(configurationFile));
        }

        configurationFile = FindConfigurationFile(configurationFile);
        var ext = PathUtils.GetFullExtension(configurationFile);

        return s_parsers.TryGetValue(ext, out var parser)
            ? parser(configurationFile)
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

    public static IConfiguration CreateOrDefaultConfiguration(string? path, IDictionary<string, string?>? @default = default)
    {
        try
        {
            return CreateConfiguration(path);
        }
        catch (FileNotFoundException)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(@default ?? new Dictionary<string, string?>())
                .Build();
        }
    }
}