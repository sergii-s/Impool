using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Impool;

namespace UsingApp
{


    [PoolObject]
    public partial class OtherStuff
    {
        public string SomeStuff { get; private set; }

        public OtherStuff(string someStuff)
        {
            SomeStuff = someStuff;
        }
    }

    [PoolObject]
    public partial class Pair
    {
        public string Id { get; private set; }
        public int Price { get; private set; }
        public List<double> SomeDoubles { get; private set; }

        private Pair(string id, int price, List<double> someDoubles)
        {
            Id = id;
            Price = price;
            SomeDoubles = someDoubles ?? new List<double>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var cnt = 0;
            Parallel.ForEach(Enumerable.Range(0, 10), x =>
            {
                while (true)
                {
                    using var pool = ObjectPoolContext.Context();

                    //            var x = pool.Get<Pair>().GetPair("x", 1);
                    //            Console.WriteLine(x.Id);
                    //            Console.WriteLine(x.Price);
                    var byteArray = pool.Get<byte[]>(() => new byte[1024], bytes => { });
                    var newCnt = Interlocked.Increment(ref cnt);
                    var pair = pool.GetOtherStuff("x");
                    var stuff = pool.GetPair("x", newCnt, new List<double>());
                    if (stuff.Price != newCnt)
                    {
                        throw new Exception();
                    }
                }    
            });
            
        }
    }
}