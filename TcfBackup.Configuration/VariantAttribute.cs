namespace TcfBackup.Configuration;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class VariantAttribute(object key, Type type) : Attribute
{
    public object Key { get; } = key;
    public Type Type { get; } = type;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class VariantAttribute<T>(object key) : VariantAttribute(key, typeof(T))
{

}