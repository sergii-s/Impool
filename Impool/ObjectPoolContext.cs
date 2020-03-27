using System;
using System.Collections.Concurrent;

namespace Impool
{
    public class ObjectPoolContext : IDisposable
    {
        private readonly ConcurrentBag<Action> _disposeActions = new ConcurrentBag<Action>();
        private ObjectPoolContext()
        {
        }

        public void Dispose()
        {
            while (_disposeActions.TryTake(out var dispose))
            {
                dispose();
            }
            GlobalPool<ObjectPoolContext>.Pool.Add(this);
        }
        
        public T Get<T>(Func<T> create, Action<T> update)
        {
            if (GlobalPool<T>.Pool.TryTake(out var item))
            {
                update(item);
                _disposeActions.Add(() => GlobalPool<T>.Pool.Add(item));
                return item;
            }

            var newItem = create();
            _disposeActions.Add(() => GlobalPool<T>.Pool.Add(newItem));
            return newItem;
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
