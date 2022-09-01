using System.Threading;
using System.Threading.Tasks;

namespace TcfBackup.Retention;

public interface IRetentionManager
{
    Task PerformCleanupAsync(CancellationToken cancellationToken);
}