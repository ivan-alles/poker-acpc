/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.metastrategy;
using NUnit.Framework;
using ai.pkr.stdpoker.generated;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.stdpoker.nunit.ref_evaluator;


namespace ai.pkr.stdpoker.nunit
{
    /// <summary>
    /// Test for CardSetEvaluator.
    /// Remarks: 
    /// 1. Checksum is used to prevent optimizing away function calls.
    /// </summary>
    [TestFixture]
    public class CardSetEvaluator_Test
    {
        #region Tests

        [Test]
        public void Test_Simple()
        {
            CardSet hand = new CardSet { bits = 0x7F};
            UInt32 handVal1 = CardSetEvaluator.Evaluate(ref hand);

            UInt32 handVal2 = RefEvaluator.Evaluate(hand.bits, 7);
            Assert.AreEqual(handVal1, handVal2);
        }

        [Test]
        public void Test_EvaluateAll5CardsCombinations()
        {
            _handTypeCounter.Reset();
            CardEnum.Combin(StdDeck.Descriptor, 5, CardSet.Empty, CardSet.Empty, Compare5Cards);
            _handTypeCounter.Verify(5);
            _handTypeCounter.Print();
        }

        [Test]
        public void Test_EvaluateAll6CardsCombinations()
        {
            _handTypeCounter.Reset();
            CardEnum.Combin(StdDeck.Descriptor, 6, CardSet.Empty, CardSet.Empty, Compare6Cards);
            _handTypeCounter.Verify(6);
            _handTypeCounter.Print();
        }

        [Test]
        public void Test_EvaluateAll7CardsCombinations()
        {
            _handTypeCounter.Reset();
            CardEnum.Combin(StdDeck.Descriptor, 7, CardSet.Empty, CardSet.Empty, Compare7Cards);
            _handTypeCounter.Verify(7);
            _handTypeCounter.Print();
        }

        [Test]
        public void Test_CountDistinctHandValues5()
        {
            for (int i = 0; i < (int) HandValue.Kind._Count + 1; ++i)
            {
                _uniqHandValues[i] = new HashSet<uint>();
            }
            CardEnum.Combin(StdDeck.Descriptor, 5, CardSet.Empty, CardSet.Empty, CountDistinctHandValues);
            Console.WriteLine("Number of unique 5-card combinations:");
            for (int i = 0; i < (int) HandValue.Kind._Count + 1; ++i)
            {
                Console.WriteLine("{0, 13}: {1,4}",
                                  ((HandValue.Kind) i).ToString(),
                                  _uniqHandValues[i].Count);
                Assert.AreEqual(_numberOfUnique5Combinations[i], _uniqHandValues[i].Count);
            }
        }

        [Test]
        public void Test_CountDistinctHandValues7()
        {
            for (int i = 0; i < (int)HandValue.Kind._Count + 1; ++i)
            {
                _uniqHandValues[i] = new HashSet<uint>();
            }
            CardEnum.Combin(StdDeck.Descriptor, 7, CardSet.Empty, CardSet.Empty, CountDistinctHandValues);
            Console.WriteLine("Number of unique 7-card combinations:");
            for (int i = 0; i < (int)HandValue.Kind._Count + 1; ++i)
            {
                Console.WriteLine("{0, 13}: {1,4}", ((HandValue.Kind)i).ToString(), _uniqHandValues[i].Count);
            }
            // Cross-verified with LutEvaluator7.
            Assert.AreEqual(4824, _uniqHandValues[(int)HandValue.Kind._Count].Count);
        }


        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_All7Cards_GenericEnumeration()
        {

            _count = 0;
            DateTime startTime = DateTime.Now;
            CardEnum.Combin(StdDeck.Descriptor, 7, CardSet.Empty, CardSet.Empty, Evaluate7Cards);
            double runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(_count, runTime, _checksum);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_All7Cards_GenericEnumerationParam()
        {
            _count = 0;
            DateTime startTime = DateTime.Now;
            CardEnum.Combin(StdDeck.Descriptor, 7, CardSet.Empty, CardSet.Empty, Evaluate7CardsParam, this);
            double runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(_count, runTime, _checksum);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_All7Cards_InlineEnumeration()
        {
            CardSet[] cardSets = StdDeck.Descriptor.CardSets;
            // Using a local variable instead of static speeds up the test!.
            int count = 0;
            UInt32 checksum = 0;
            DateTime startTime = DateTime.Now;
            CardSet cs = new CardSet();
            UInt64 m1, m2, m3, m4, m5, m6;
            for (int c1 = 52 - 1; c1 >= 6; --c1)
            {
                m1 = cardSets[c1].bits;
                for (int c2 = c1 - 1; c2 >= 5; --c2)
                {
                    m2 = m1 | cardSets[c2].bits;
                    for (int c3 = c2 - 1; c3 >= 4; --c3)
                    {
                        m3 = m2 | cardSets[c3].bits;
                        for (int c4 = c3 - 1; c4 >= 3; --c4)
                        {
                            m4 = m3 | cardSets[c4].bits;
                            for (int c5 = c4 - 1; c5 >= 2; --c5)
                            {
                                m5 = m4 | cardSets[c5].bits;
                                for (int c6 = c5 - 1; c6 >= 1; --c6)
                                {
                                    m6 = m5 | cardSets[c6].bits;
                                    for (int c7 = c6 - 1; c7 >= 0; --c7)
                                    {
                                        count++;
                                        cs.bits = m6 | cardSets[c7].bits;
                                        checksum += CardSetEvaluator.Evaluate(ref cs);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            double runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(count, runTime, checksum);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_2Players_GenericEnumeration()
        {
            // 1 Player has AcKd, 2 has 2h2s
            _count = 0;
            _player1 = StdDeck.Descriptor.GetCardSet("Ac Kd");
            _player2 = StdDeck.Descriptor.GetCardSet("2h 2s");
            CardSet dead = _player1 | _player2;

            int REPETITIONS = 50;
            _win = _lose = _tie = 0;
            DateTime startTime = DateTime.Now;
            for (int r = 0; r < REPETITIONS; ++r)
            {
                CardEnum.Combin(StdDeck.Descriptor, 5, CardSet.Empty, dead, Evaluate2Players);
            }
            double runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(_count * 2, runTime, 0);
            Assert.AreEqual(1712304 * REPETITIONS, _count);
            Assert.AreEqual(799119 * REPETITIONS, _win);
            Assert.AreEqual(903239 * REPETITIONS, _lose);
            Assert.AreEqual(9946 * REPETITIONS, _tie);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_3Players_GenericEnumeration()
        {
            // 1st Player has KcKh, 2nd has 7s6s, 3rd has AcQc
            _count = 0;
            _player1 = StdDeck.Descriptor.GetCardSet("Kc Kh");
            _player2 = StdDeck.Descriptor.GetCardSet("7s 6s");
            _player3 = StdDeck.Descriptor.GetCardSet("Ac Qc");
            CardSet dead = _player1 | _player2 | _player3;

            int REPETITIONS = 20;
            _win = _lose = _tie = 0;
            DateTime startTime = DateTime.Now;
            for (int r = 0; r < REPETITIONS; ++r)
            {
                CardEnum.Combin(StdDeck.Descriptor, 5, CardSet.Empty, dead, Evaluate3Players);
            }
            double runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(_count * 3, runTime, 0);
            Assert.AreEqual(1370754 * REPETITIONS, _count);
            Assert.AreEqual(683919 * REPETITIONS, _win);
            Assert.AreEqual(683386 * REPETITIONS, _lose);
            Assert.AreEqual(3449 * REPETITIONS, _tie);
        }



        [Test]
        [Category("Benchmark")]
        public void Benchmark_RandomCards()
        {
            int handCount = 1000000;
#if DEBUG
            int repCount = 1;
#else
            int repCount = 40;
#endif
            Console.WriteLine("Random hands: {0}, repetitions: {1}, total: {2}", handCount, repCount, handCount * repCount);

            RandomHandGenerator randomHands = new RandomHandGenerator();
            randomHands.Generate(handCount);
            randomHands.SetMask(5);
            Console.WriteLine("{0} random 5-hands generated", handCount);

            DateTime startTime;
            double runTime;
            UInt32 checksum = 0;
            startTime = DateTime.Now;
            for (int r = 0; r < repCount; ++r)
            {
                for (int i = 0; i < handCount; ++i)
                {
                    UInt32 value = CardSetEvaluator.Evaluate(ref randomHands.hands[i].CardSet);
                    checksum += value;
#if DEBUG
                    VerifyHandValue(RefEvaluator.Evaluate(randomHands.hands[i].CardSet.bits,
                        randomHands.hands[i].CardSet.CountCards()), value);
#endif
                }
            }
            runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(handCount * repCount, runTime, checksum);

            randomHands.SetMask(7);

            Console.WriteLine("\n{0} random 7-hands generated", handCount);

            startTime = DateTime.Now;
            for (int r = 0; r < repCount; ++r)
            {
                for (int i = 0; i < handCount; ++i)
                {
                    UInt32 value = CardSetEvaluator.Evaluate(ref randomHands.hands[i].CardSet);
                    checksum += value;
#if DEBUG
                    VerifyHandValue(RefEvaluator.Evaluate(randomHands.hands[i].CardSet.bits, 7), value);
#endif
                }
            }
            runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(handCount * repCount, runTime, checksum);
        }


        #endregion

        #region Implementation

        void VerifyHandValue(UInt32 handValExp, UInt32 handValAct)
        {
            if (handValExp != handValAct)
            {// Set a breakpoint here
            }
            Assert.AreEqual(handValExp, handValAct);
        }

        private void Compare7Cards(ref CardSet mask)
        {
            _count++;
            UInt32 handVal1 = CardSetEvaluator.Evaluate(ref mask);
            UInt32 handVal2 = RefEvaluator.Evaluate(mask.bits, 7);
            // Use direct check, it is faster than NUnit assert, and for 133M repetitions it makes a difference.
            if (handVal1 != handVal2)
            {
                Assert.Fail();
            }
            _handTypeCounter.Count(handVal1);
        }

        private void Compare6Cards(ref CardSet mask)
        {
            _count++;
            UInt32 handVal1 = CardSetEvaluator.Evaluate(ref mask);
            UInt32 handVal2 = RefEvaluator.Evaluate(mask.bits, 6);
            if (handVal1 != handVal2)
            {
            }
            Assert.IsTrue(0 < handVal1);
            Assert.AreEqual(handVal1, handVal2);
            _handTypeCounter.Count(handVal1);
        }

        private void Compare5Cards(ref CardSet mask)
        {
            _count++;
            UInt32 handVal1 = CardSetEvaluator.Evaluate(ref mask);
            UInt32 handVal2 = RefEvaluator.Evaluate(mask.bits, 5);
            if (handVal1 != handVal2)
            {
            }
            Assert.IsTrue(0 < handVal1);
            Assert.AreEqual(handVal1, handVal2);
            _handTypeCounter.Count(handVal1);
        }

        private static void CountDistinctHandValues(ref CardSet mask)
        {
            UInt32 handVal = CardSetEvaluator.Evaluate(ref mask);
            CountHandValues(handVal);
        }

        private static void CountHandValues(uint handVal)
        {
            HandValue.Kind handType = HandValue.GetKind(handVal);
            _uniqHandValues[(int) handType].Add(handVal);
            // Count total as well
            _uniqHandValues[(int)HandValue.Kind._Count].Add(handVal);
        }

        private int _count = 0;
        private UInt32 _checksum = 0;

        private static HashSet<UInt32>[] _uniqHandValues = new HashSet<UInt32>[(int) HandValue.Kind._Count + 1];
        static HandTypeCounter _handTypeCounter = new HandTypeCounter();

        // Source: Cactus Kev, http://www.suffecool.net/poker/evaluator.html
        private static readonly UInt32[] _numberOfUnique5Combinations =
        {
            1277,
            2860,
            858,
            858,
            10,
            1277,
            156,
            156,
            10,
            7462
        };

        [TestFixtureSetUp]
        public void Setup()
        {
            // Call enumerations once to force JITing.
            CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, CardSet.Empty, Evaluate7Cards);
            CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, CardSet.Empty, Evaluate7CardsParam, this);
        }

        void Evaluate7Cards(ref CardSet mask)
        {
            _count++;
            UInt32 handVal = CardSetEvaluator.Evaluate(ref mask);
            _checksum += handVal;
        }

        void Evaluate7CardsParam(ref CardSet mask, CardSetEvaluator_Test param)
        {
            param._count++;
            UInt32 handVal = CardSetEvaluator.Evaluate(ref mask);
            _checksum += handVal;
        }


        private int _win, _lose, _tie;
        private CardSet _player1, _player2, _player3;

        void Evaluate2Players(ref CardSet handMask)
        {
            _count++;
            CardSet hand1 = handMask | _player1;
            CardSet hand2 = handMask | _player2;

            UInt32 val1 = CardSetEvaluator.Evaluate(ref hand1);
            UInt32 val2 = CardSetEvaluator.Evaluate(ref hand2);
            if (val1 > val2) _win++;
            else if (val1 < val2) _lose++;
            else _tie++;
        }


        void Evaluate3Players(ref CardSet handMask)
        {
            _count++;
            CardSet hand1 = handMask | _player1;
            CardSet hand2 = handMask | _player2;
            CardSet hand3 = handMask | _player3;

            UInt32 val1 = CardSetEvaluator.Evaluate(ref hand1);
            UInt32 val2 = CardSetEvaluator.Evaluate(ref hand2);
            UInt32 val3 = CardSetEvaluator.Evaluate(ref hand3);
            if (val1 > val2 && val1 > val3) _win++;
            else if (val1 < val2 || val1 < val3) _lose++;
            else _tie++;
        }

        private void PrintResult(int count, double duration, uint checksum)
        {
            Console.WriteLine("{0:###.###} s, {1:###,###,###,###} h/s, {2} comb, checksum: {3}",
                duration, count / duration, count, checksum);
        }

        #endregion
    }
}
