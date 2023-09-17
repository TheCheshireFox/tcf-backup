using System;
using TcfBackup.Shared.ProgressLogger;

namespace TcfBackup.Factory;

public class EmptyProgressLoggerFactory : IProgressLoggerFactory
{
    private class EmptyProgressLogger : IProgressLogger
    {
#pragma warning disable CS0067
        public event Action<long>? OnProgress;
#pragma warning restore CS0067

        public void Set(long value)
        {
        }

        public void Add(long value)
        {
        }
    }

    public IProgressLogger Create(long threshold) => new EmptyProgressLogger();
}

public class ProgressLoggerFactory : IProgressLoggerFactory
{
    public IProgressLogger Create(long threshold) => new ProgressLogger(threshold);
}