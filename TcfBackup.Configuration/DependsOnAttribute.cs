namespace TcfBackup.Configuration;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DependsOnAttribute(Type type, string name, object key) : Attribute
{
    public Type Type { get; } = type;
    public string Name { get; } = name;
    public object Key { get; } = key;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DependsOnAttribute<T>(string name, object key) : DependsOnAttribute(typeof(T), name, key)
{
    
}