using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TcfBackup.Extensions.Configuration;

public static class ServiceCollectionConfigurationExtensions
{
    public static IServiceCollection Configure<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]T>(this IServiceCollection collection, IConfiguration configuration, Func<IConfiguration, T> binder)
        where T : class
        => collection.AddSingleton<IOptions<T>>(_ => new OptionsWrapper<T>(binder(configuration)));
}