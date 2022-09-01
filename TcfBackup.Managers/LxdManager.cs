using TcfBackup.Shared;

namespace TcfBackup.Managers;

public class LxdManager : ILxdManager
{
    public void CheckAvailable()
    {
        if (SystemUtils.Which("lxc") == null)
        {
            throw new FileNotFoundException("Lxc executable not found");
        }
    }
    
    public string[] ListContainers() => Subprocess.Exec("lxc", "list -c n -f csv").Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    public void BackupContainer(string container, string targetFile) => Subprocess.Exec("lxc", $"export --debug -v --instance-only {container} {targetFile}");
}