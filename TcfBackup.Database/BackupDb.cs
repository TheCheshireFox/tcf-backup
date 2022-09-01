using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.SqlQuery;

namespace TcfBackup.Database;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
internal class BackupDb : DataConnection
{
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2075")]
    static BackupDb()
    {
        DefaultSettings = new LinqToDbSettings();
        
        var db = new BackupDb();
        
        var schemaProvider = db.DataProvider.GetSchemaProvider();
        var dbSchema = schemaProvider.GetSchema(db);

        var tables = db.GetType()
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(ITable<>))
            .ToDictionary(db.GetTableName, db.GetTableType);

        foreach (var table in tables.Where(table => dbSchema.Tables.All(t => t.TableName != table.Key)))
        {
            db.CreateTable(table.Value);
        }
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

    }

    public ITable<Backup> Backup => this.GetTable<Backup>();
    
    public ITable<BackupFile> BackupFiles => this.GetTable<BackupFile>();
}