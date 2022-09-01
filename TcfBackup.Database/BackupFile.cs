using System.Diagnostics.CodeAnalysis;
using LinqToDB.Mapping;

namespace TcfBackup.Database;

[Table]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
internal class BackupFile
{
    [PrimaryKey, Identity]
    public int Id { get; set; }
    
    [Column]
    public int BackupId { get; set; }
    
    [Column]
    public string Path { get; set; } = null!;
}