using TcfBackup.Source;

namespace TcfBackup.Action;

public interface IActionContext
{
    bool TryGetFileListSource(out IFileListSource source);
    bool TryGetStreamSource(out IStreamSource source);
    
    void SetResult(IFileListSource source);
    void SetResult(IStreamSource source);
}