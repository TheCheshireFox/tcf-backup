namespace TcfBackup.Database.Repository;

public class Backup
{
    public int Id { get; internal set; }
    public string Name { get; set; } = null!;
    public DateTime Date { get; set; }
    public IEnumerable<BackupFile> Files { get; set; } = Enumerable.Empty<BackupFile>();
}