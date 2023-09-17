using System;
using TcfBackup.Shared.ProgressLogger;

namespace TcfBackup.Factory;

public class EmptyProgressLoggerFactory : IProgressLoggerFactory
{
    private class EmptyProgressLogger : IProgressLogger
    {
        public event Action<long>? OnProgress;
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