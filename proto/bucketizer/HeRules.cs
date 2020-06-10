using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem.strategy.core;

namespace bucketizer_proto
{
    class HeRules : IRules
    {
        private static readonly string[] _cards;

        static readonly int[] _cardCounts = new int[169];


        static HeRules()
        {
            _cards = new string[HePocket.Count];
            for(int p = 0; p < HePocket.Count; ++p)
            {
                _cards[p] = HePocket.KindToString((HePocketKind)p);
                _cardCounts[p] = HePocket.KindToRange((HePocketKind)p).Length;
            }
        }

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
            HePocketKind [] prange1 = new HePocketKind[range1.Length];
            HePocketKind [] prange2 = new HePocketKind[range2.Length];
            for(int i = 0; i < range1.Length; ++i)
            {
                prange1[i] = (HePocketKind) range1[i];
            }
            for (int i = 0; i < range2.Length; ++i)
            {
                prange2[i] = (HePocketKind)range2[i];
            }
            PocketEquity.Result r = PocketEquity.CalculateFast(prange1, prange2);
            return r.Equity;
        }
    }
}
