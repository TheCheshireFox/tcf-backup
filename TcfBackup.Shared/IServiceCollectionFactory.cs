namespace TcfBackup.Shared;

public interface IServiceCollectionFactory<out T>
{
    T Create();
}