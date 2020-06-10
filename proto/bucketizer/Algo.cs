using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bucketizer_proto
{
    class Algo
    {
        public IRules Rules;
        public Bucket[] Buckets;
        public int Step = 0;
        public int NotMovedCount = 0;
        public double BestSumDist = 0;


        public void Print()
        {
            PrintHeader();
            PrintBuckets();
        }

        void PrintHeader()
        {
            Console.WriteLine("Step {0,3} ---------------------------------------------------------------------------", Step);
        }

        void PrintBuckets()
        {
            for (int b = 0; b < Buckets.Length; ++b )
            {
                Console.Write("   {0}: ", b);
                Buckets[b].Print(Rules);
                Console.WriteLine();
            }
            Console.WriteLine("Best sum dist: {0}", BestSumDist);
        }

        public void UpdateAverage()
        {
            for (int b1 = 0; b1 < Buckets.Length; ++b1)
            {
                Buckets[b1].AvResult = new List<double>();
                for (int b2 = 0; b2 < Buckets.Length; ++b2)
                {
                    double av = CalcAverageForBucket(b1, b2);
                    Buckets[b1].AvResult.Add(av);
                }
            }
        }

        public double UpdateDistFromAverage()
        {
            double sumDistance = 0;
            for (int b1 = 0; b1 < Buckets.Length; ++b1)
            {
                Buckets[b1].SumDistFromAverage = 0;
                foreach (int card in Buckets[b1].Cards)
                {
                    List<double> avVector = new List<double>();
                    for (int b2 = 0; b2 < Buckets.Length; ++b2)
                    {
                        double av = CalcAverageForCard(card, b2);
                        avVector.Add(av);
                    }
                    double d = GetDistance(Buckets[b1].AvResult, avVector/*, b1*/);
                    Buckets[b1].SumDistFromAverage += d * Rules.CardCounts[card];
                }
                //Buckets[b1].SumDistFromAverage /= Buckets[b1].Cards.Count;
                sumDistance += Buckets[b1].SumDistFromAverage;
            }
            return sumDistance;
        }

        public double CalcAverageForBucket(int b1, int b2)
        {
            return Rules.Equity(Buckets[b1].Cards.ToArray(), Buckets[b2].Cards.ToArray());
        }

        public double CalcAverageForCard(int c1, int b2)
        {
            return Rules.Equity(new int []{c1}, Buckets[b2].Cards.ToArray());
        }



        public void DoStep()
        {
            PrintHeader();

            int card = Step % Rules.Cards.Length;

            //Console.WriteLine(card);
            Step++;
            int curBucket = FindCard(card);
            if (Buckets[curBucket].Cards.Count == 1)
            {
                Console.WriteLine("Do not take single card");
                return;
            }

            UpdateAverage();
            double bestDist = UpdateDistFromAverage();
            int bestBucket = curBucket;

            Buckets[curBucket].Cards.Remove(card);

            for (int b = 0; b < Buckets.Length; ++b)
            {
                if(b == curBucket)
                {
                    continue;
                }

                Buckets[b].Cards.Add(card);
                UpdateAverage();
                double dist = UpdateDistFromAverage();
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestBucket = b;
                }
                Console.WriteLine("{0}: {1} -> {2}", Rules.Cards[card], curBucket, b);
                PrintBuckets();
                Console.WriteLine("Cur sum dist: {0}", dist);


                Buckets[b].Cards.Remove(card);
            }
            Buckets[bestBucket].Cards.Add(card);
            UpdateAverage();
            BestSumDist = UpdateDistFromAverage();
            Console.WriteLine("{0}: {1} -> {2}", Rules.Cards[card], curBucket, bestBucket);
            if(bestBucket == curBucket)
            {
                NotMovedCount++;
            }
            else
            {
                PrintBuckets();
                NotMovedCount = 0;
            }
        }

        public bool IsStable()
        {
            return NotMovedCount == Rules.Cards.Length;
        }

        double GetDistance(List<double> v1, List<double> v2)
        {
            double dist = 0;
            for (int i = 0; i < v1.Count; ++i)
            {
                double d = v1[i] - v2[i];
                dist += d*d;
            }
            return Math.Sqrt(dist);
        }

        double GetDistance(List<double> v1, List<double> v2, int skip)
        {
            double dist = 0;
            for (int i = 0; i < v1.Count; ++i)
            {
                if (i == skip)
                {
                    continue;
                }
                double d = v1[i] - v2[i];
                dist += d * d;
            }
            return Math.Sqrt(dist);
        }

        int FindCard(int card)
        {
            for (int b = 0; b < Buckets.Length; ++b)
            {
                if (Buckets[b].Cards.Exists(c => c == card))
                {
                    return b;
                }
            }
            throw new ApplicationException();
        }

        public void SortBuckets()
        {
            foreach(Bucket b in Buckets)
            {
                b.Cards.Sort();
            }
        }


    }
}
