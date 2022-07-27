using System.Collections.Generic;

namespace TcfBackup.Database;

public interface IBackupDatabase
{
    void Add(BackupInfo backupInfo);
    IEnumerable<BackupInfo> GetAll();
}