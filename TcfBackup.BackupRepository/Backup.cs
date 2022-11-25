namespace TcfBackup.BackupDatabase;

public class Backup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime Date { get; set; }
    public List<BackupFile> Files { get; set; } = Enumerable.Empty<BackupFile>().ToList();
}