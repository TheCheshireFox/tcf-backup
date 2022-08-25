using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.SqlQuery;

namespace TcfBackup.Database;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public class BackupDb : DataConnection
{
    static BackupDb()
    {
        DefaultSettings = new LinqToDbSettings();
    }

    private void CreateTable(Type tableType)
    {
        var dataExtensions = typeof(DataExtensions);
        var createTableMethod = dataExtensions
            .GetMethod(nameof(DataExtensions.CreateTable))!
            .MakeGenericMethod(tableType);

        createTableMethod.Invoke(null, new object?[]
        {
            this,
            null,
            null,
            null,
            null,
            null,
            DefaultNullable.None,
            null,
            default(TableOptions)
        });
    }
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ITable<>))]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2075")]
    private string GetTableName(PropertyInfo propertyInfo)
    {
        var table = propertyInfo.GetValue(this)!;
        return (string)table.GetType().GetProperty(nameof(ITable<object>.TableName))!.GetValue(table)!;
    }

    private Type GetTableType(PropertyInfo propertyInfo) =>
        propertyInfo.PropertyType.GetGenericArguments()[0];
    
    public BackupDb()
        : base(ProviderName.SQLite)
    {
        var schemaProvider = DataProvider.GetSchemaProvider();
        var dbSchema = schemaProvider.GetSchema(this);

        var tables = GetType()
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(ITable<>))
            .ToDictionary(GetTableName, GetTableType);

        foreach (var table in tables.Where(table => dbSchema.Tables.All(t => t.TableName != table.Key)))
        {
            CreateTable(table.Value);
        }
    }

    public ITable<Backup> Backup => this.GetTable<Backup>();
}