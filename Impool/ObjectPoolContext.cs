using System;
using System.Collections.Concurrent;

namespace Impool
{
    internal static class GlobalPool<T>
    {
        public static readonly ConcurrentBag<T> Pool = new ConcurrentBag<T>();
    }
    public class GenericPool<T> : IDisposable
    {
        private readonly ConcurrentBag<T> _objectsPool;
        private readonly ConcurrentBag<T> _usingObjects = new ConcurrentBag<T>();

        public GenericPool(ConcurrentBag<T> objectsPool)
        {
            _objectsPool = objectsPool;
        }

        public void Dispose()
        {
            while (_usingObjects.TryTake(out var usingObject))
            {
                _objectsPool.Add(usingObject);
            }
            GlobalPool<GenericPool<T>>.Pool.Add(this);
        }
        
        public T Create(Func<T> create, Action<T> update)
        {
            var item = GetOrCreate();
            _usingObjects.Add(item);
            return item;
            
            T GetOrCreate()
            {
                if (_objectsPool.TryTake(out var objectFromPool))
                {
                    update(objectFromPool);
                    return objectFromPool;
                }
                return create();
            }
        }
    }
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
