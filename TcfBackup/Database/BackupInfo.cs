using System;
using TcfBackup.Database.Action;
using TcfBackup.Database.Source;
using TcfBackup.Database.Target;

namespace TcfBackup.Database;

public class BackupInfo
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public IRestoreSource RestoreSource { get; set; }
    public IRestoreAction[] RestoreActions { get; set; }
    public IRestoreTarget RestoreTarget { get; set; }
}