/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;
using NUnit.Framework;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.lib.algorithms.random;
using ai.pkr.metastrategy.nunit;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for CardEnum. 
    /// </summary>
    [TestFixture]
    public class CardEnum_Test
    {
        #region Tests

        [Test]
        public void Test_CountCombinations_CardSet()
        {
            for (int i = 0; i <= 6; ++i)
            {
                _combinationsCount = 0;
                CardEnum.Combin(StdDeck.Descriptor, i, CardSet.Empty, CardSet.Empty, CountCombinations);
                Assert.AreEqual(EnumAlgos.CountCombin(52, i), _combinationsCount);

                _combinationsCount1 = 0;
                CardEnum.Combin(StdDeck.Descriptor, i, CardSet.Empty, CardSet.Empty, CountCombinationsParam, _combinationsCount);
                Assert.AreEqual(EnumAlgos.CountCombin(52, i), _combinationsCount1);
            }
        }

        [Test]
        public void Test_CountCombinations_Idx()
        {
            int[] cards = new int[7];
            for (int i = 0; i <= 6; ++i)
            {
                _combinationsCount1 = 0;
                CardEnum.Combin(StdDeck.Descriptor, i, cards, 0, null, 0, CountCombinationsParam, 33);
                Assert.AreEqual(EnumAlgos.CountCombin(52, i), _combinationsCount1);
            }
        }

        [Test]
        public void Test_Random()
        {
            int REPETITIONS = 100;
#if DEBUG
            REPETITIONS = 8;
#endif

            int seed = (int)DateTime.Now.Ticks;
            Console.WriteLine("Random seed {0}", seed);
            Random rand = new Random(seed);
            _cardRng = new SequenceRng(seed);
            _cardRng.SetSequence(StdDeck.Descriptor.FullDeckIndexes);
            for (int r = 0; r < REPETITIONS; ++r)
            {
                _enumCount = r % 7;
                int sharedCount = rand.Next(0, StdDeck.Descriptor.Size + 1 - _enumCount);
                int deadCount = rand.Next(0, StdDeck.Descriptor.Size + 1 - sharedCount - _enumCount);

                _cardRng.Shuffle(sharedCount + deadCount);
                int rc = 0;

                _shared = new CardSet();
                for (int i = 0; i < sharedCount; ++i)
                {
                    _shared |= StdDeck.Descriptor.CardSets[_cardRng.Sequence[rc++]];
                }

                _dead = new CardSet();
                for (int i = 0; i < deadCount; ++i)
                {
                    _dead |= StdDeck.Descriptor.CardSets[_cardRng.Sequence[rc++]];
                }
                Debug.Assert(rc == sharedCount + deadCount);
                Debug.Assert(!_shared.IsIntersectingWith(_dead));

                //Console.WriteLine("B: {0:x16} D:{1:x16}", board, dead);
                _combinationsCount = 0;
                _lastCs = 0;
                CardEnum.Combin(StdDeck.Descriptor, _enumCount, _shared, _dead, VerifyCombination);
                Assert.AreEqual(EnumAlgos.CountCombin(StdDeck.Descriptor.Size - sharedCount - deadCount, _enumCount), _combinationsCount);

                _combinationsCount1 = 0;
                _lastCs1 = 0;
                CardEnum.Combin(StdDeck.Descriptor, _enumCount, _shared, _dead, VerifyCombinationParam, _combinationsCount);
                Assert.AreEqual(EnumAlgos.CountCombin(StdDeck.Descriptor.Size - sharedCount - deadCount, _enumCount), _combinationsCount1);

                _combinationsCount1 = 0;
                _lastCs1 = 0;
                int[] cards = new int[_enumCount + sharedCount].Fill(-1);
                StdDeck.Descriptor.GetIndexesAscending(_shared ).ToArray().CopyTo(cards, 0);
                int[] deadIdx = StdDeck.Descriptor.GetIndexesAscending(_shared | _dead).ToArray();

                CardEnum.Combin(StdDeck.Descriptor, _enumCount, cards, sharedCount, deadIdx, deadIdx.Length, VerifyCombinationParam, _combinationsCount);
                Assert.AreEqual(EnumAlgos.CountCombin(StdDeck.Descriptor.Size - sharedCount - deadCount, _enumCount), _combinationsCount1);

            }
            Console.WriteLine("{0} repetitions done.", REPETITIONS);
        }


        [Test]
        public void Test_CombinArray_CardSet()
        {
            CardSet[] arr = CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, CardSet.Empty);
            Assert.AreEqual(1326, arr.Length);
            CardSet prev = arr[0];
            Assert.AreEqual(2, prev.CountCards());
            for(int i = 1; i < arr.Length; ++i)
            {
                CardSet cur = arr[i];
                Assert.Less(prev.bits, cur.bits);
                Assert.AreEqual(2, cur.CountCards());
                prev = cur;
            }
        }


        #endregion

        #region Benchmark

        [Test]
        public void Benchmark_CountCombinations_CardSet()
        {
            int count;
            DateTime startTime;
            double runTime;

            count = 0;
            startTime = DateTime.Now;
            CardEnum.Combin(StdDeck.Descriptor, 7, CardSet.Empty, CardSet.Empty, (ref CardSet cs) => { count++;});
            runTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Cardset (no parameters): {0:#,#} combinations, {1:0.0} s, {2:#,#} comb/s", count, runTime,
                              count/runTime);

            count = 0;
            startTime = DateTime.Now;
            CardEnum.Combin(StdDeck.Descriptor, 7, CardSet.Empty, CardSet.Empty, (ref CardSet cs, CardEnum_Test t) => { count++; }, this);
            runTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Cardset (object parameter): {0:#,#} combinations, {1:0.0} s, {2:#,#} comb/s", count, runTime,
                              count / runTime);
        }

        [Test]
        public void Benchmark_CountCombinations_Idx()
        {
            int count;
            DateTime startTime;
            double runTime;
            int[] cards = new int[7];

            count = 0;
            startTime = DateTime.Now;
            CardEnum.Combin(StdDeck.Descriptor, 7, cards, 0, null, 0, (int[] c, CardEnum_Test t) => { count++; }, this);
            runTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Cardset (object parameter): {0:#,#} combinations, {1:0.0} s, {2:#,#} comb/s", count, runTime,
                              count / runTime);
        }

        #endregion

        #region Implementation

        private void CountCombinations(ref CardSet mask)
        {
            _combinationsCount++;
        }
        private void CountCombinationsParam(ref CardSet mask, int param)
        {
            Assert.AreEqual(_combinationsCount, param);
            _combinationsCount1++;
        }

        private void CountCombinationsParam(int [] cards, int param)
        {
            Assert.AreEqual(33, param);
            _combinationsCount1++;
        }

        private void VerifyCombination(ref CardSet cs)
        {
            _combinationsCount++;
            Assert.IsFalse(cs.IsIntersectingWith(_dead));
            Assert.IsTrue(cs.Contains(_shared));
            CardSet uniqueMask = new CardSet { bits = cs.bits & (~_shared.bits)};
            Assert.AreEqual(_enumCount, uniqueMask.CountCards());
            // Use the fact that CardEnum generate masks in ascending order to check uniqueness
            if (_enumCount > 0)
            {
                Assert.Greater(uniqueMask.bits, _lastCs);
            }
            _lastCs = uniqueMask.bits;
        }

        private void VerifyCombinationParam(ref CardSet cs, int param)
        {
            Assert.AreEqual(_combinationsCount, param);
            _combinationsCount1++;
            Assert.IsFalse(cs.IsIntersectingWith(_dead));
            Assert.IsTrue(cs.Contains(_shared));
            CardSet uniqueMask = new CardSet { bits = cs.bits & (~_shared.bits) };
            Assert.AreEqual(_enumCount, uniqueMask.CountCards());
            // Use the fact that CardEnum generate masks in ascending order to check uniqueness
            if (_enumCount > 0)
            {
                Assert.Greater(uniqueMask.bits, _lastCs1);
            }
            _lastCs1 = uniqueMask.bits;
        }

        private void VerifyCombinationParam(int [] cards, int param)
        {
            CardSet cs = StdDeck.Descriptor.GetCardSet(cards);
            Assert.AreEqual(_combinationsCount, param);
            _combinationsCount1++;
            Assert.IsFalse(cs.IsIntersectingWith(_dead));
            Assert.IsTrue(cs.Contains(_shared));
            CardSet uniqueCs = new CardSet { bits = cs.bits & (~_shared.bits) };
            Assert.AreEqual(_enumCount, uniqueCs.CountCards());
            // Use the fact that CardEnum generate masks in ascending order to check uniqueness
            if (_enumCount > 0)
            {
                Assert.Greater(uniqueCs.bits, _lastCs1);
            }
            _lastCs1 = uniqueCs.bits;
        }


        private int _combinationsCount = 0;
        private int _combinationsCount1 = 0;
        private int _enumCount;
        CardSet _shared = new CardSet();
        CardSet _dead = new CardSet();
        private UInt64 _lastCs;
        private UInt64 _lastCs1;
        private SequenceRng _cardRng;

        #endregion
    }
}