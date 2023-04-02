namespace TcfBackup.Source;

public interface ISource
{
    void Prepare(CancellationToken cancellationToken);
    void Cleanup(CancellationToken cancellationToken);
}