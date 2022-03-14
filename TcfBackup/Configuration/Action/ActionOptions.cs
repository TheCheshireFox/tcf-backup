namespace TcfBackup.Configuration.Action
{
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
        public ActionType Type { get; set; }
    }
}