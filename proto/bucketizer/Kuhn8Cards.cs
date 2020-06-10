using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bucketizer_proto
{
    class Kuhn8Cards : IRules
    {
        static readonly string[] _cards = new string[]
        {
            "T1",
            "T2",
            "J1",
            "J2",
            "Q1",
            "Q2",
            "K1",
            "K2"
        };

        static readonly int[] _cardCounts = new int[] { 1,1,1,1,1,1,1,1};

        public int[] CardCounts
        {
            get { return _cardCounts; }
        }

        public string[] Cards
        {
            get { return _cards; }
        }

        public double Equity(int[] range1, int[] range2)
        {
            int sum = 0;
            int count = 0;
            foreach(int c1 in range1)
            {
                foreach (int c2 in range2)
                {
                    count++;
                    sum += Showdown(c1, c2);
                }
            }
            return (double)sum / count;
        }


        int Showdown(int i1, int i2)
        {
            string c1 = Cards[i1];
            string c2 = Cards[i2];

            int r1 = "TJQK".IndexOf(c1.Substring(0, 1));
            int r2 = "TJQK".IndexOf(c2.Substring(0, 1));
            if (r1 == 0 || r2 == 0)
            {
                return 0; // T always ties to every card.
            }
            if (r1 < r2)
            {
                return -1;
            }
            if (r1 > r2)
            {
                return 1;
            }
            return 0;
        }
    }
}
