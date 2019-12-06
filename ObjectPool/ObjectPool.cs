using System.Collections.Concurrent;

namespace ObjectPool
{
    public class ObjectPool
    {
        public ObjectPoolContext Context()
        {
            return new ObjectPoolContext();
        }
    }
}