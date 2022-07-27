using System;
using System.Collections.Generic;
using TcfBackup.Database.Action;

namespace TcfBackup.Database.JsonDatabase;

public class RestoreActionJsonConverter : PropertyDependentJsonConverter<RestoreActionType, IRestoreAction>
{
    private static readonly Dictionary<RestoreActionType, Type> s_mapping = new()
    {
        { RestoreActionType.Decompress, typeof(DecompressRestoreAction) },
        { RestoreActionType.Decrypt, typeof(DecryptRestoreAction) }
    };

    public RestoreActionJsonConverter() : base("Type", s_mapping)
    {
    }
}