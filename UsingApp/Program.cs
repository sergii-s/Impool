using System;
using ObjectPool;

namespace UsingApp
{
    
    [PoolObject]
    public class Pair
    {
        public string Id { get; }
        public double Price { get; }

        public Pair(string id, double price)
        {
            Id = id;
            Price = price;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var pool = new ObjectPool.ObjectPool();

            using var ctx = pool.Context();

            var p = new Pair();
            var x = ctx.Get<Pair>();
        }
    }
}