using System;
using System.Collections.Generic;
using TcfBackup.Database.Source;

namespace TcfBackup.Database.JsonDatabase;

public class RestoreSourceJsonConverter : PropertyDependentJsonConverter<RestoreSourceType, IRestoreSource>
{
    private static readonly Dictionary<RestoreSourceType, Type> s_mapping = new()
    {
        { RestoreSourceType.Directory, typeof(DirRestoreSource) },
        { RestoreSourceType.GDrive, typeof(GDriveRestoreSource) }
    };

    public RestoreSourceJsonConverter() : base("Type", s_mapping)
    {
    }
}