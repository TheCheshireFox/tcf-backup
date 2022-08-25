using System.Diagnostics.CodeAnalysis;
using LinqToDB.Mapping;

namespace TcfBackup.Database;

[Table]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public class Backup
{
    [PrimaryKey, Identity]
    public int BackupId { get; set; }
    
    [Column]
    public DateTime Date { get; set; }
    
    [Column]
    public string Path { get; set; } = null!;
}