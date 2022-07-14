using Microsoft.Extensions.DependencyInjection;
using TcfBackup.Managers;

namespace TcfBackup.Factory.Manager;

public abstract class ManagerFactoryScoped<TManager, TSelector>
    where TManager: IManager
{
    private readonly IServiceCollection _serviceCollection;
    
    public ManagerFactoryScoped(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    TManager Create(TSelector selector)
    {
        
    }
    
    protected abstract TManager CreateInstance(TSelector selector);
}