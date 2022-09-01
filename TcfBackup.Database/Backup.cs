using System.Diagnostics.CodeAnalysis;
using LinqToDB.Mapping;

namespace TcfBackup.Database;

[Table]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
internal class Backup
{
    [PrimaryKey, Identity]
    public int Id { get; set; }
    
    [Column, LinqToDB.Mapping.NotNull]
    public string Name { get; set; } = null!;

    [Column]
    public DateTime Date { get; set; }
    
    [Association(ThisKey = nameof(Id), OtherKey = nameof(BackupFile.Id))]
    public IEnumerable<BackupFile> Files { get; set; } = Enumerable.Empty<BackupFile>();
}