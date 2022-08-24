using LinqToDB.Mapping;

namespace TcfBackup.Database;

[Table]
public class Backup
{
    [PrimaryKey, Identity]
    public int BackupId { get; set; }
    
    [Column]
    public DateTime Date { get; set; }
    
    [Column]
    public string Path { get; set; }
}