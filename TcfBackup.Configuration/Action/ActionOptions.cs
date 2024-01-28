using TcfBackup.Configuration.Action.CompressAction;
using TcfBackup.Configuration.Action.EncryptAction;

namespace TcfBackup.Configuration.Action;

public enum ActionType
{
    None,
    Compress,
    Encrypt,
    Filter,
    Rename
}

public class ActionOptions
{
    [Variant<FilterActionOptions>(ActionType.Filter)]
    [Variant<RenameActionOptions>(ActionType.Rename)]
    [Variant<EncryptActionOptions>(ActionType.Encrypt)]
    [Variant<CompressActionOptions>(ActionType.Compress)]
    public ActionType Type { get; set; }
}