/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.stdpoker;
using ai.pkr.metagame;
using System.Diagnostics;
using System.IO;

namespace ai.pkr.research
{
    /// <summary>
    /// An unfinished attempt to write an enumerator that will generate combination
    /// of suit-normalized cards. I think this is possible, but probably will be not much (if any) faster
    /// than a combination of a usual enumerator and SuitNormalizer.
    /// </summary>
    [TestFixture]
    public class SuitNormCombin_Test
    {
        #region Tests

        [Test]
        public void Test_2()
        {
            Verbose = false;
            Assert.AreEqual(169, Combin(2));
            Verbose = false;
            //Assert.AreEqual(16432, Combin(4));
            _list.Clear();
            Combin(4);
            _list.Sort(new CardSetComparer());
            using (TextWriter tw = new StreamWriter("comb4.txt"))
            {
                foreach (CardSet cs in _list)
                {
                    tw.WriteLine(cs.ToString());
                }
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class CardSetComparer: IComparer<CardSet>
        {

            #region IComparer<CardSet> Members

            public int Compare(CardSet x, CardSet y)
            {
                if (x.bits < y.bits) return -1;
                else if (x.bits > y.bits) return 1;
                return 0;
            }

            #endregion
        }

        bool Verbose = true;

        private List<CardSet> _list = new List<CardSet>();

        int Combin(int n)
        {
            int deckSize = StdDeck.Descriptor.Size;
            int suitCount = 4;
            CardSet[] deck = new CardSet[52];
            int i = 0;
            for (int r = 12; r >= 0; --r)
            {
                for (int s = 0; s < 4; ++s)
                {
                    deck[i++] = StdDeck.Descriptor.CardSets[StdDeck.IndexFromRankAndSuit(r, s)];
                }
            }
            int counter = 0;
            CombinRecursive(deck, suitCount, deckSize, n, 0, CardSet.Empty, -1, suitCount-1, -1, ref counter);
            return counter;
        }

        void CombinRecursive(
            // Deck parameters
            CardSet[] deck, int suitCount, int deckSize, 
            // Initial call parameters
            int n,
            // Algorithm parameters
            int depth, CardSet result1, int r1, int s1, int maxSuit,
            // Debug and test parameters
            ref int counter
            )
        {
            if (depth == n)
            {
                Debug.Assert(result1.CountCards() == n);
                counter++;
                _list.Add(result1);
                if (Verbose)
                {
                    Console.WriteLine("{0,6}: {1}", counter, result1);
                }
                return;
            }
            int r2 = r1;
            int s2 = s1 + 1;
            if (s2 >= suitCount)
            {
                s2 = 0;
                r2++;
            }

            for (; (r2 + s2) < deckSize + 1 - (n - depth); )
            {
                int maxSuit2 = r2 == r1 ? maxSuit : Math.Max(maxSuit, s2);
                //int maxSuit2 = Math.Max(maxSuit, s2);
                CardSet result2 = deck[r2 + s2];
                Debug.Assert(!result2.IsIntersectingWith(result1));
                result2.UnionWith(result1);
                CombinRecursive(deck, suitCount, deckSize, n, depth + 1, result2, r2, s2, maxSuit2, ref counter);
                s2++;
                if (s2 > Math.Min(maxSuit + 1, suitCount-1))
                {
                    s2 = 0;
                    r2 += suitCount;
                }
            }
        }

        #endregion
    }
}
