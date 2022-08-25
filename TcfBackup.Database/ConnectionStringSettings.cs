using LinqToDB.Configuration;

namespace TcfBackup.Database;

internal class ConnectionStringSettings : IConnectionStringSettings
{
    public string ConnectionString { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string ProviderName { get; init; } = null!;
    public bool IsGlobal => false;
}