using System;
using System.Threading;

namespace TcfBackup;

public class InterruptionHandler
{
    private readonly CancellationTokenSource _cts = new();

    public CancellationToken Token => _cts.Token;
    
    public InterruptionHandler()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => _cts.Cancel();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _cts.Cancel();
        };
    }
}