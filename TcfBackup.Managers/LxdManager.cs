using TcfBackup.Shared;

namespace TcfBackup.Managers
{
    public class LxdManager : ILxdManager
    {
        public string[] ListContainers() => Subprocess.Exec("lxc", "list -c n -f csv").Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        public void BackupContainer(string container, string targetFile) => Subprocess.Exec("lxc", $"export --instance-only {container} {targetFile}");
    }
}