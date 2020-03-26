using System.Collections.Concurrent;

namespace Impool
{
    internal static class GlobalPool<T>
    {
        public static readonly ConcurrentBag<T> Pool = new ConcurrentBag<T>();
    }
}