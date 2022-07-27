using System;
using System.Collections.Generic;
using TcfBackup.Database.Target;

namespace TcfBackup.Database.JsonDatabase;

public class RestoreTargetJsonConverter : PropertyDependentJsonConverter<RestoreTargetType, IRestoreTarget>
{
    private static readonly Dictionary<RestoreTargetType, Type> s_mapping = new()
    {
        { RestoreTargetType.Directory, typeof(DirRestoreTarget) }
    };

    public RestoreTargetJsonConverter() : base("Type", s_mapping)
    {
    }
}