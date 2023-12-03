namespace TcfBackup.Configuration.Action;

public class RenameActionOptions : ActionOptions
{
    public string Template { get; set; } = string.Empty;
    public bool Overwrite { get; set; }
}