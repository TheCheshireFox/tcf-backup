namespace TcfBackup.LxdClient.Operation;

public class LxdOperation
{
    internal string Url { get; }

    protected LxdOperation(string operationUrl)
    {
        Url = operationUrl;
    }
}