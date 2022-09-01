using System;
using System.Collections.Generic;

namespace TcfBackup.Test;

public class FuncEqualityComparer<TValue> : IEqualityComparer<TValue>
{
    private readonly Func<TValue, TValue, bool> _comparer;

    public FuncEqualityComparer(Func<TValue, TValue, bool> comparer)
    {
        _comparer = comparer;
    }

    public bool Equals(TValue? x, TValue? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null && y != null || x != null && y == null)
        {
            return false;
        }

        return _comparer(x!, y!);
    }

    public int GetHashCode(TValue obj) => obj?.GetHashCode() ?? 0;
}