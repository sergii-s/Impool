using System;
using System.Collections.Concurrent;

public class ObjectPoolContext : IDisposable
{
    public void Dispose()
    {
        
    }

    public T Get<T>()
    {
        return default;
    }
}