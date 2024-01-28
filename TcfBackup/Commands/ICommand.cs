using Microsoft.Extensions.DependencyInjection;
using TcfBackup.CommandLine.Options;

namespace TcfBackup.Commands;

public interface ICommand<T> where T: GenericOptions
{
    IServiceCollection CreateServiceCollection(GenericOptions opts);
    void Invoke(T opts, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}