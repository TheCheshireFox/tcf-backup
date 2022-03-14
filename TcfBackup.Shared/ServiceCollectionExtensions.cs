using Microsoft.Extensions.DependencyInjection;

namespace TcfBackup.Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletonFromFactory<TFactory, T>(this IServiceCollection serviceCollection)
            where TFactory: class, IServiceCollectionFactory<T>
            where T: class
        {
            return serviceCollection
                .AddSingleton<TFactory>()
                .AddSingleton(sp => sp.GetService<TFactory>()!.Create());
        }
        
        public static IServiceCollection AddTransientFromFactory<TFactory, T>(this IServiceCollection serviceCollection)
            where TFactory: class, IServiceCollectionFactory<T>
            where T: class
        {
            return serviceCollection
                .AddTransient<TFactory>()
                .AddTransient(sp => sp.GetService<TFactory>()!.Create());
        }
    }
}