using System.Diagnostics;
using TcfBackup.Shared.Native.Unix;

namespace TcfBackup.Shared
{
    public delegate void ProcessRedirects(StreamWriter input, StreamReader output, StreamReader error);

    public class ProcessException : Exception
    {
        public int ExitCode { get; set; }

        public ProcessException(string message, int exitCode = -1)
            : base(message)
        {
            ExitCode = exitCode;
        }
    }

    public static class Subprocess
    {
        public static string Exec(string command, string args, TimeSpan timeout, IDictionary<string, string>? env = null, CancellationToken cancellationToken = default)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            if (env != null)
            {
                foreach (var (envName, envValue) in env)
                {
                    process.StartInfo.Environment.Add(envName, envValue);
                }
            }

            process.StartInfo.Environment.Add("LC_ALL", "C");

            if (!process.Start())
            {
                throw new ProcessException("Unable to start process");
            }

            using var processKiller = new UnixProcessKiller(process, timeout, cancellationToken);

            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                process.Kill(true);
                throw new TimeoutException();
            }

            if (process.ExitCode != 0)
            {
                throw new ProcessException($"Process failed: {process.StandardError.ReadToEnd()}", process.ExitCode);
            }

            return process.StandardOutput.ReadToEnd();
        }

        public static string Exec(string command, string args, IDictionary<string, string>? env = null, CancellationToken cancellationToken = default)
        {
            return Exec(command, args, Timeout.InfiniteTimeSpan, env, cancellationToken);
        }

        public static void Exec(string command, string args, TimeSpan timeout, ProcessRedirects? redirects, IDictionary<string, string>? env = null, CancellationToken cancellationToken = default)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                },
                EnableRaisingEvents = true
            };

            if (env != null)
            {
                foreach (var (envName, envValue) in env)
                {
                    process.StartInfo.Environment.Add(envName, envValue);
                }
            }

            process.StartInfo.Environment.Add("LC_ALL", "C");

            if (!process.Start())
            {
                throw new ProcessException("Unable to start process");
            }

            using var processKiller = new UnixProcessKiller(process, timeout, cancellationToken);

            var redirectsTask = redirects == null
                ? Task.CompletedTask
                : Task.Factory.StartNew(() => redirects(process.StandardInput, process.StandardOutput, process.StandardError), TaskCreationOptions.LongRunning);

            var processTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            process.Exited += (_, _) => processTcs.SetResult();

            if (!Task.WaitAll(new[] { redirectsTask, processTcs.Task }, (int)timeout.TotalMilliseconds, cancellationToken))
            {
                process.Kill(true);
                throw new TimeoutException();
            }

            if (process.ExitCode != 0)
            {
                throw new ProcessException($"Process failed: {process.StandardError.ReadToEnd()}", process.ExitCode);
            }
        }

        public static void Exec(string command, string args, ProcessRedirects redirects, IDictionary<string, string>? env = null, CancellationToken cancellationToken = default)
        {
            Exec(command, args, Timeout.InfiniteTimeSpan, redirects, env, cancellationToken);
        }
    }
}