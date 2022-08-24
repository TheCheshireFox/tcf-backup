using LinqToDB;
using LinqToDB.Configuration;

namespace TcfBackup.Database;

internal class LinqToDbSettings : ILinqToDBSettings
{
    public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();
    public string DefaultConfiguration => ProviderName.SQLite;
    public string DefaultDataProvider => ProviderName.SQLite;

    public IEnumerable<IConnectionStringSettings> ConnectionStrings
    {
        get
        {
            yield return new ConnectionStringSettings
            {
                Name = ProviderName.SQLite,
                ProviderName = ProviderName.SQLite,
                ConnectionString = $@"Data Source={GetDbPath()};Version=3"
            };
        }
    }

    private static string GetDbPath() =>
        Path.Combine(AppEnvironment.TcfPersistentDirectory, "backup.db");
}