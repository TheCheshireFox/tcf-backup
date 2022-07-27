using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TcfBackup.Database.JsonDatabase;

public class LocalJsonBackupDatabase : IBackupDatabase
{
    private static JsonSerializerSettings s_settings = new JsonSerializerSettings()
    {
        Converters = new List<JsonConverter>(new[]
        {
            (JsonConverter)new RestoreSourceJsonConverter(),
            new RestoreActionJsonConverter(),
            new RestoreTargetJsonConverter()
        })
    };
    
    private readonly string _dbPath;

    private List<BackupInfo> Load() =>
        JsonConvert.DeserializeObject<List<BackupInfo>>(File.ReadAllText(_dbPath), s_settings) ?? new List<BackupInfo>();

    public LocalJsonBackupDatabase(string dbPath)
    {
        _dbPath = dbPath;

        if (!File.Exists(_dbPath))
        {
            File.WriteAllText(_dbPath, "");
        }
    }
    
    public void Add(BackupInfo backupInfo)
    {
        var backups = Load();
        
        if (backups.Any(b => b.Name == backupInfo.Name && b.Date == backupInfo.Date))
        {
            throw new ArgumentException();
        }
        
        backups.Add(backupInfo);
        File.WriteAllText(_dbPath, JsonConvert.SerializeObject(backups));
    }

    public IEnumerable<BackupInfo> GetAll() => Load();
}