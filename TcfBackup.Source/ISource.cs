namespace TcfBackup.Source;

public interface ISource
{
    void Prepare();
    void Cleanup();
}