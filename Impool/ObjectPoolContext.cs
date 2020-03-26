using System;
using System.Collections.Concurrent;

namespace Impool
{
    public class ObjectPoolContext : IDisposable
    {
        private ObjectPoolContext()
        {
        }

        private readonly ConcurrentDictionary<Type, IDisposable> _genericPools = new ConcurrentDictionary<Type, IDisposable>();
        
        public void Dispose()
        {
            foreach (var pool in _genericPools)
            {
                pool.Value.Dispose();   
            }
            _genericPools.Clear();
            GlobalPool<ObjectPoolContext>.Pool.Add(this);
        }

        private GenericPool<T> GetPool<T>()
        { 
            return (GenericPool<T>)_genericPools.GetOrAdd(typeof(T), type =>
            {
                if (GlobalPool<GenericPool<T>>.Pool.TryTake(out var poolFromPool))
                {
                    return poolFromPool;
                }
                return new GenericPool<T>(GlobalPool<T>.Pool);
            });
        }
        public T Get<T>(Func<T> create, Action<T> update)
        {
            return GetPool<T>().Create(create, update);
        }
        
        public static ObjectPoolContext Context()
        {
            if (GlobalPool<ObjectPoolContext>.Pool.TryTake(out var poolFromPool))
            {
                return poolFromPool;
            }
            return new ObjectPoolContext();
        }
    }

}
