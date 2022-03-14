using System.Reflection;

namespace TcfBackup.Shared;

public static class ServiceProviderExtensions
{
    private static bool ResolveConstructor(IServiceProvider provider, ConstructorInfo constructorInfo, IDictionary<Type, object> createdObjects, out object[] args)
    {
        object? GetOrAddObject(Type type)
        {
            if (createdObjects.TryGetValue(type, out var obj))
            {
                return obj;
            }

            obj = provider.GetService(type);
            if (obj == null)
            {
                return null;
            }

            createdObjects.Add(type, obj);
            return obj;
        }

        var constructorParams = constructorInfo.GetParameters();
        args = new object[constructorParams.Length];

        for (var i = 0; i < constructorParams.Length; i++)
        {
            var arg = GetOrAddObject(constructorParams[i].ParameterType);
            if (arg == null)
            {
                return false;
            }

            args[i] = arg;
        }

        return true;
    }

    public static T CreateService<T>(this IServiceProvider provider)
    {
        var createdObjects = new Dictionary<Type, object>();

        foreach (var constructor in typeof(T).GetConstructors().OrderBy(c => c.GetParameters().Length))
        {
            if (constructor.GetParameters().Length == 0)
            {
                return Activator.CreateInstance<T>();
            }

            if (ResolveConstructor(provider, constructor, createdObjects, out var args))
            {
                return (T?)Activator.CreateInstance(typeof(T), args) ?? throw new InvalidOperationException();
            }
        }

        throw new InvalidOperationException($"Unable to create service of type {typeof(T)}");
    }
}