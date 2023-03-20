namespace TcfBackup.Source;

public class StreamSource : IStreamSource
{
    private readonly Stream _stream;

    public string Name { get; set; }
    
    public StreamSource(Stream stream, string name)
    {
        _stream = stream;
        Name = name;
    }

    public void Prepare()
    {
        
    }

    public void Cleanup()
    {
        _stream.Close();
    }

    public Stream GetStream() => _stream;
}