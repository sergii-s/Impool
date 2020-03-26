using System;
using System.Collections.Concurrent;

namespace Impool
{
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
}