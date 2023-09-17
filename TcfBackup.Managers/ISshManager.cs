namespace TcfBackup.Managers;

public interface ISshManager
{
    void Upload(Stream src, string dst, bool overwrite, CancellationToken cancellationToken);
    void Delete(string path, CancellationToken cancellationToken);
}