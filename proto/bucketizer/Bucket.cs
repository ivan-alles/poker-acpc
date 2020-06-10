using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bucketizer_proto
{
    class Bucket
    {
        public List<int> Cards = new List<int>();
        public List<double> AvResult = new List<double>();
        public double SumDistFromAverage;
        
        public void Print(IRules rules)
        {
            foreach (int card in Cards)
            {
                Console.Write(rules.Cards[card] + " ");
            }
            Console.Write("sdfa: {0:0.000} r: ", SumDistFromAverage);

            foreach (double ar in AvResult)
            {
                Console.Write("{0,5:0.00} ", ar);
            }
        }
    }
}
