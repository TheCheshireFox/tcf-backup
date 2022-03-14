namespace TcfBackup.Configuration.Target
{
    public class DirectoryTargetOptions : TargetOptions
    {
        public string Path { get; set; } = string.Empty;
        public bool Overwrite { get; set; }
    }
}