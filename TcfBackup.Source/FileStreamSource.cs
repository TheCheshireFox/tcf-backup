using TcfBackup.Filesystem;

namespace TcfBackup.Source;

public class FileStreamSource : IStreamSource
{
    private readonly IFileSystem _fs;
    private readonly FileStream _stream;
    private readonly bool _temp;

    public string Name { get; set; }

    public FileStreamSource(IFileSystem fs, FileStream stream, bool temp)
    {
        _fs = fs;
        _stream = stream;
        _temp = temp;

        Name = stream.Name;
    }

    public void Prepare(CancellationToken cancellationToken)
    {
        
    }

    public void Cleanup(CancellationToken cancellationToken)
    {
        _stream.Close();
        
        if (!_temp)
        {
            return;
        }
        
        _fs.File.Delete(_stream.Name);
    }

    public Stream GetStream() => _stream;
}