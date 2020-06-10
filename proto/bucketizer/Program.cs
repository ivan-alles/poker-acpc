using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem.strategy.core;
using ai.lib.algorithms;
using ai.lib.algorithms.random;

namespace bucketizer_proto
{
    class Program
    {
        static public void InitKuhn6(Algo algo)
        {
            algo.Buckets = new Bucket[3];
            algo.Buckets[0] = new Bucket();
            algo.Buckets[0].Cards.Add(0);
            algo.Buckets[0].Cards.Add(4);

            algo.Buckets[1] = new Bucket();
            algo.Buckets[1].Cards.Add(1);
            algo.Buckets[1].Cards.Add(2);

            algo.Buckets[2] = new Bucket();
            algo.Buckets[2].Cards.Add(3);
            algo.Buckets[2].Cards.Add(5);
        }

        static public void InitKuhn8(Algo algo)
        {
            algo.Buckets = new Bucket[4];
            algo.Buckets[0] = new Bucket();
            algo.Buckets[0].Cards.Add(2);
            algo.Buckets[0].Cards.Add(6);

            algo.Buckets[1] = new Bucket();
            algo.Buckets[1].Cards.Add(3);
            algo.Buckets[1].Cards.Add(4);

            algo.Buckets[2] = new Bucket();
            algo.Buckets[2].Cards.Add(5);
            algo.Buckets[2].Cards.Add(0);

            algo.Buckets[3] = new Bucket();
            algo.Buckets[3].Cards.Add(7);
            algo.Buckets[3].Cards.Add(1);
        }

        static public void InitHe(Algo algo, int bucketsCount, bool shuffle)
        {
            algo.Buckets = new Bucket[bucketsCount].Fill(i => new Bucket());

            SequenceRng shuffler = new SequenceRng();
            shuffler.SetSequence(169);
            if (shuffle)
            {
                shuffler.Shuffle();
            }

            double handsInBucket = HePocket.Count/bucketsCount;

            for(int p = 0; p < HePocket.Count; ++p)
            {
                int b = (int)(p/handsInBucket);
                if (b >= bucketsCount)
                {
                    b = bucketsCount - 1;
                }
                algo.Buckets[b].Cards.Add(shuffler.Sequence[p]);
            }
        }

        static void Main(string[] args)
        {
            int stepsCount = 1000;
            if(args.Length >= 1)
            {
                stepsCount = int.Parse(args[0]);
            }
            Algo a = new Algo();

            //a.Rules = new Kuhn6Cards();
            //InitKuhn6(a);

            //a.Rules = new Kuhn8Cards();
            //InitKuhn8(a);

            a.Rules = new HeRules();
            InitHe(a, 20, true);
            
            a.UpdateAverage();
            a.UpdateDistFromAverage();
            a.Print();

            for (int s = 0; s < stepsCount; ++s)
            {
                a.DoStep();
                if(a.IsStable())
                {
                    Console.WriteLine("STABLE!!!");
                    break;
                }
            }
            Console.WriteLine("Sorted:");
            a.SortBuckets();
            a.Print();
        }
    }
}
