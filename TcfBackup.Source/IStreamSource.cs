namespace TcfBackup.Source;

public interface IStreamSource : ISource
{
    string Name { get; set;  }
    Stream GetStream();
}