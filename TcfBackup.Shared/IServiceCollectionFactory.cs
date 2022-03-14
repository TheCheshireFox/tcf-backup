namespace TcfBackup.Shared
{
    public interface IServiceCollectionFactory<T>
    {
        T Create();
    }
}