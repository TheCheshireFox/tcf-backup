using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TcfBackup.Shared;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TcfBackup.Configuration
{
    public static class ConfigurationFactory
    {
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

            switch (PathUtils.GetFullExtension(configurationFile))
            {
                case ".yaml":
                {
                    var deserializer = new DeserializerBuilder()
                        .IgnoreUnmatchedProperties()
                        .WithNamingConvention(UnderscoredNamingConvention.Instance)
                        .Build();

                    using var ms = new MemoryStream();
                    ms.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deserializer.Deserialize(new StreamReader(configurationFile)))));
                    ms.Seek(0, SeekOrigin.Begin);

                    return new ConfigurationBuilder()
                        .AddJsonStream(ms)
                        .Build();
                }
                case ".json":
                {
                    using var file = File.OpenRead(configurationFile);

                    return new ConfigurationBuilder()
                        .AddJsonStream(file)
                        .Build();
                }
                default:
                    throw new NotSupportedException(PathUtils.GetFullExtension(configurationFile));
            }
        }

        public static IConfiguration CreateBackupConfiguration(BackupOptions? opts) => ReadConfiguration(opts?.ConfigurationFile);
        public static IConfiguration CreateRestoreConfiguration(RestoreOptions? opts) => ReadConfiguration(opts?.ConfigurationFile);
    }
}