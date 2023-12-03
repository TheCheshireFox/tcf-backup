using NUnit.Framework;
using TcfBackup.Action;
using TcfBackup.Source;

namespace TcfBackup.Test.Action;

public class ActionContextMock : IActionContext
{
    private readonly IFileListSource? _fileListSource;
    private readonly IStreamSource? _streamSource;

    private ISource? _result;
    
    public ActionContextMock(IFileListSource? fileListSource = null, IStreamSource? streamSource = null)
    {
        _fileListSource = fileListSource;
        _streamSource = streamSource;
    }

    public bool TryGetFileListSource(out IFileListSource source)
    {
        return (source = _fileListSource!) != null;
    }

    public bool TryGetStreamSource(out IStreamSource source)
    {
        return (source = _streamSource!) != null;
    }

    public void SetResult(IFileListSource source)
    {
        _result = source;
    }

    public void SetResult(IStreamSource source)
    {
        _result = source;
    }

    public T GetResult<T>()
        where T : ISource
    {
        if (_result == null)
        {
            Assert.Fail("Result is not set.");
            return default!;
        }
        
        if (_result is not T typedResult)
        {
            Assert.Fail($"Not a {typeof(T).Name}");
            return default!;
        }

        return typedResult;
    }
}