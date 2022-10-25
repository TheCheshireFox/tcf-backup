namespace TcfBackup.LxdClient.Operation;

public class LxdOperation
{
    internal string Url { get; }
    
    public LxdOperation(string operationUrl)
    {
        Url = operationUrl;
    }
}