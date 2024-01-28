using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using TcfBackup.Configuration.Action;
using TcfBackup.Configuration.Source;
using TcfBackup.Configuration.Target;
using TcfBackup.Extensions.Configuration;

namespace TcfBackup.Configuration;

public class ConfigurationProvider : IConfigurationProvider
{
    private const string SourceKey = "source";
    private const string TargetKey = "target";
    private const string ActionsKey = "actions";

    private readonly IConfiguration _configuration;

    public ConfigurationProvider(IConfiguration configuration)
    {
        _configuration = configuration;
        
        if (_configuration == null)
        {
            throw new FormatException("Invalid configuration");
        }
        
        if (!_configuration.ContainsKey(SourceKey)) throw new FormatException("Source not specified");
        if (!_configuration.ContainsKey(TargetKey)) throw new FormatException("Target not specified");
        if (!_configuration.ContainsKey(ActionsKey)) throw new FormatException("Actions not specified");
    }
    
    public SourceOptions GetSource()
    {
        return (SourceOptions)Get(typeof(SourceOptions), _configuration.GetSection(SourceKey), new ConfigurationState());
    }

    public TargetOptions GetTarget()
    {
        return (TargetOptions)Get(typeof(TargetOptions), _configuration.GetSection(TargetKey), new ConfigurationState());
    }

    public IEnumerable<ActionOptions> GetActions()
    {
        return _configuration
            .GetSection(ActionsKey)
            .GetChildren()
            .Select(actionSection => (ActionOptions)Get(typeof(ActionOptions), actionSection, new ConfigurationState()));
    }
    
    private static object Get(Type type, IConfiguration configuration, ConfigurationState state)
    {
        var obj = configuration.Get(type);
        var props = type.GetProperties().ToDictionary(p => p.Name, p => p);
        
        foreach (var prop in props.Values)
        {
            if (!state.VariantsApplied.Contains(prop.Name))
            {
                var variants = prop
                    .GetCustomAttributes<VariantAttribute>()
                    .Select(a => a.Key.GetType() == prop.PropertyType ? a : throw new Exception($"Variant key type {a.Key.GetType()} != property type {prop.PropertyType}"))
                    .ToDictionary(a => a.Key, a => a.Type);
            
                var propValue = prop.GetValue(obj);
                if (propValue == null)
                {
                    continue;
                }
            
                if (variants.TryGetValue(propValue, out var newType))
                {
                    if (type != newType && state.VariantsApplied.Add(prop.Name))
                    {
                        return Get(newType, configuration, state);
                    }
                }
            }

            foreach (var dependsOn in prop.GetCustomAttributes<DependsOnAttribute>())
            {
                if (props[dependsOn.Name].PropertyType != dependsOn.Key.GetType())
                {
                    throw new Exception($"Dependency key type {dependsOn.Key.GetType()} != target property type {props[dependsOn.Name].PropertyType}");
                }

                var depType = props[dependsOn.Name].PropertyType;
                
                if (!state.Comparers.TryGetValue(depType, out var comparer))
                {
                    state.Comparers[depType] = comparer = (IEqualityComparer)typeof(EqualityComparer<>)
                        .MakeGenericType(depType)
                        .GetProperty("Default", BindingFlags.Static | BindingFlags.Public)!
                        .GetValue(null)!;
                }

                if (!comparer.Equals(dependsOn.Key, props[dependsOn.Name].GetValue(obj)))
                {
                    continue;
                }
                
                prop.SetValue(obj, Get(dependsOn.Type, configuration.GetSection(prop.Name), state));
                break;
            }
        }

        return obj!;
    }

    private class ConfigurationState
    {
        public Dictionary<Type, IEqualityComparer> Comparers { get; } = new();
        public HashSet<string> VariantsApplied { get; } = [];
    }
}