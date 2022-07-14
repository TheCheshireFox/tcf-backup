namespace TcfBackup.Shared;

public static class SystemUtils
{
    public static string? Which(string exec)
    {
        try
        {
            return Subprocess.Exec("which", exec);
        }
        catch (ProcessException)
        {
            return null;
        }
    }
}