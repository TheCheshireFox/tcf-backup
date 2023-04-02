using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace TcfBackup.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonFromFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TFactory, T>(this IServiceCollection serviceCollection)
        where TFactory : class, IServiceCollectionFactory<T>
        where T : class
    {
        return serviceCollection
            .AddSingleton<TFactory>()
            .AddSingleton(sp => sp.GetService<TFactory>()!.Create());
    }

    public static IServiceCollection AddTransientFromFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TFactory, T>(this IServiceCollection serviceCollection)
        where TFactory : class, IServiceCollectionFactory<T>
        where T : class
    {
        return serviceCollection
            .AddTransient<TFactory>()
            .AddTransient(sp => sp.GetService<TFactory>()!.Create());
    }
}