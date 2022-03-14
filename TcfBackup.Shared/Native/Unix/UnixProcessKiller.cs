using System.Diagnostics;
using Mono.Unix.Native;

namespace TcfBackup.Shared.Native.Unix
{
    public class UnixProcessKiller : IProcessKiller
    {
        private readonly CancellationTokenRegistration? _tokenRegistration;

        public UnixProcessKiller(Process process, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.CanBeCanceled) return;

            _tokenRegistration = cancellationToken.Register(() =>
            {
                if (process.HasExited)
                {
                    return;
                }

                if (process.Id > 1)
                {
                    Syscall.kill(process.Id, Signum.SIGINT);
                }

                if (!process.WaitForExit((int)timeout.TotalMilliseconds))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                        // NOP
                    }
                }

                _tokenRegistration?.Dispose();
            });
        }

        public void Dispose()
        {
            _tokenRegistration?.Dispose();
        }
    }
}