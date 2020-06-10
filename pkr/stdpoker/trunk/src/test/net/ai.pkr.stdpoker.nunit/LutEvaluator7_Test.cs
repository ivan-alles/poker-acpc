/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ai.pkr.metagame;
using ai.pkr.stdpoker.nunit.ref_evaluator;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.stdpoker.nunit
{
    /// <summary>
    /// Unit tests for LutEvaluator7. 
    /// Remarks: 
    /// 1. Checksum is used to prevent optimizing away function calls.
    /// </summary>
    [TestFixture]
    public unsafe class LutEvaluator7_Test
    {
        #region Tests

        [Test]
        public void Test_Simple()
        {
            string handS = "As Ks Kd Kc 2c 2d 5c";
            int[] handA = StdDeck.Descriptor.GetIndexes(handS);
            CardSet handC = StdDeck.Descriptor.GetCardSet(handS);
            UInt32 val1 = LutEvaluator7.Evaluate(handA);
            UInt32 val2 = RefEvaluator.Evaluate(handC.bits, 7);
            Assert.AreEqual(val2, val1);
        }

        /// <summary>
        /// Generate table manually.
        /// </summary>
        [Test]
        [Explicit]
        public void Test_Generate()
        {
            LutEvaluatorGenerator g = new LutEvaluatorGenerator();
            g.GenerateStates(7);
            Assert.IsTrue(g.Test7Hands());
            string lutPath = LutEvaluator7.LutPath;
            g.SaveLut(lutPath, LutEvaluator7.LutFileFormatID);
        }

        [Test]
        public void Test_EvaluateAll7CardsCombinations()
        {
            _handTypeCounter.Reset();
            int [] handA = new int[7];
            GenerateTestCombin(CardSet.Empty, handA, 0, 0, EvaluateAndVerify);
            _handTypeCounter.Verify(7);
            _handTypeCounter.Print();
        }

        [Test]
        public void Test_RandomCards_Array()
        {
            int handCount = 1000000;
            int rngSeed = (int)DateTime.Now.Ticks;
            Console.WriteLine("Seed: {0}", rngSeed);
            RandomHandGenerator randomHands = new RandomHandGenerator(rngSeed);
            randomHands.Generate(handCount);
            randomHands.SetMask(7);
            for (int i = 0; i < handCount; ++i)
            {
                UInt32 value = LutEvaluator7.Evaluate(randomHands.hands[i].Cards);
                VerifyHandValue(RefEvaluator.Evaluate(randomHands.hands[i].CardSet.bits, 7), value);
            }
        }


        [Test]
        [Explicit]
        public void Test_CountDistinctHandValues7()
        {
            HashSet<UInt32> distinct = new HashSet<uint>();
            for (int c1 = 52 - 1; c1 >= 6; --c1)
            {
                UInt32 v1 = LutEvaluator7.pLut[c1];
                for (int c2 = c1 - 1; c2 >= 5; --c2)
                {
                    UInt32 v2 = LutEvaluator7.pLut[v1 + c2];
                    for (int c3 = c2 - 1; c3 >= 4; --c3)
                    {
                        UInt32 v3 = LutEvaluator7.pLut[v2 + c3];
                        for (int c4 = c3 - 1; c4 >= 3; --c4)
                        {
                            UInt32 v4 = LutEvaluator7.pLut[v3 + c4];
                            for (int c5 = c4 - 1; c5 >= 2; --c5)
                            {
                                UInt32 v5 = LutEvaluator7.pLut[v4 + c5] + (uint)c5 - 1;
                                for (int c6 = c5 - 1; c6 >= 1; --c6, --v5)
                                {
                                    UInt32 v6 = LutEvaluator7.pLut[v5] + (uint)c6 - 1;
                                    for (int c7 = c6 - 1; c7 >= 0; --c7, --v6)
                                    {
                                        UInt32 rank = LutEvaluator7.pLut[v6];
                                        distinct.Add(rank);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Number of 7-card distinct hand values: {0:0,0}", distinct.Count);
            // Cross-verified with CardSetEvaluator.
            Assert.AreEqual(4824, distinct.Count);
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_All7Cards_Incremental()
        {
            CardSet[] cardSets = StdDeck.Descriptor.CardSets;
            int count = 0;
            UInt32 checksum = LutEvaluator7.Evaluate(0, 1, 2, 3, 4, 5, 6);
            DateTime startTime = DateTime.Now;
            for (int c1 = 52 - 1; c1 >= 6; --c1)
            {
                UInt32 v1 = LutEvaluator7.pLut[c1];
                for (int c2 = c1 - 1; c2 >= 5; --c2)
                {
                    UInt32 v2 = LutEvaluator7.pLut[v1 + c2];
                    for (int c3 = c2 - 1; c3 >= 4; --c3)
                    {
                        UInt32 v3 = LutEvaluator7.pLut[v2 + c3];
                        for (int c4 = c3 - 1; c4 >= 3; --c4)
                        {
                            UInt32 v4 = LutEvaluator7.pLut[v3 + c4];
                            for (int c5 = c4 - 1; c5 >= 2; --c5)
                            {
                                UInt32 v5 = LutEvaluator7.pLut[v4 + c5] + (uint)c5 - 1; 
                                for (int c6 = c5 - 1; c6 >= 1; --c6, --v5)
                                {
                                    UInt32 v6 = LutEvaluator7.pLut[v5] + (uint)c6 - 1;
                                    for (int c7 = c6 - 1; c7 >= 0; --c7, --v6)
                                    {
                                        count++;
                                        UInt32 value = LutEvaluator7.pLut[v6];
                                        checksum += value;
#if DEBUG
                                        int [] handA = new int[]{c1, c2, c3, c4, c5, c6, c7};
                                        CardSet handC = StdDeck.Descriptor.GetCardSet(handA);
                                        VerifyHandValue(RefEvaluator.Evaluate(handC.bits, 7), value);
#endif
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
        public void Benchmark_All7Cards_Indexes()
        {
            CardSet[] cardSets = StdDeck.Descriptor.CardSets;
            // Using a local variable instead of static speeds up the test!.
            int count = 0;
            UInt32 checksum = LutEvaluator7.Evaluate(0,1,2,3,4,5,6);
            DateTime startTime = DateTime.Now;
            for (int c1 = 52 - 1; c1 >= 6; --c1)
            {
                for (int c2 = c1 - 1; c2 >= 5; --c2)
                {
                    for (int c3 = c2 - 1; c3 >= 4; --c3)
                    {
                        for (int c4 = c3 - 1; c4 >= 3; --c4)
                        {
                            for (int c5 = c4 - 1; c5 >= 2; --c5)
                            {
                                for (int c6 = c5 - 1; c6 >= 1; --c6)
                                {
                                    for (int c7 = c6 - 1; c7 >= 0; --c7)
                                    {
                                        count++;
                                        UInt32 value = LutEvaluator7.Evaluate(c1, c2, c3, c4, c5, c6, c7);
                                        checksum += value;
#if DEBUG
                                        int [] handA = new int[]{c1, c2, c3, c4, c5, c6, c7};
                                        CardSet handC = StdDeck.Descriptor.GetCardSet(handA);
                                        VerifyHandValue(RefEvaluator.Evaluate(handC.bits, 7), value);
#endif
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
        public void Benchmark_All7Cards_Array()
        {
            int count = 0;
            // Force loading and JIT.
            UInt32 checksum = LutEvaluator7.Evaluate(new int[]{0,1,2,3,4,5,6});
            int [] hand = new int[7];
            DateTime startTime = DateTime.Now;
            for (hand[0] = 52 - 1; hand[0] >= 6; --hand[0])
            {
                for (hand[1] = hand[0] - 1; hand[1] >= 5; --hand[1])
                {
                    for (hand[2] = hand[1] - 1; hand[2] >= 4; --hand[2])
                    {
                        for (hand[3] = hand[2] - 1; hand[3] >= 3; --hand[3])
                        {
                            for (hand[4] = hand[3] - 1; hand[4] >= 2; --hand[4])
                            {
                                for (hand[5] = hand[4] - 1; hand[5] >= 1; --hand[5])
                                {
                                    for (hand[6] = hand[5] - 1; hand[6] >= 0; --hand[6])
                                    {
                                        count++;
                                        UInt32 value = LutEvaluator7.Evaluate(hand);
                                        checksum += value;
#if DEBUG
                                        CardSet handC = StdDeck.Descriptor.GetCardSet(hand);
                                        VerifyHandValue(RefEvaluator.Evaluate(handC.bits, 7), value);
#endif
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
        public void Benchmark_RandomCards_Array()
        {
            int handCount = 1000000;
#if DEBUG
            int repCount = 1;
#else
            int repCount = 20;
#endif
            Console.WriteLine("Random hands: {0}, repetitions: {1}, total: {2}", handCount, repCount, handCount * repCount);

            RandomHandGenerator randomHands = new RandomHandGenerator();
            randomHands.Generate(handCount);


            // Force loading and JIT.
            UInt32 checksum = LutEvaluator7.Evaluate(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            randomHands.SetMask(7);

            Console.WriteLine("\n{0} random 7-hands generated", handCount);

            DateTime startTime = DateTime.Now;
            for (int r = 0; r < repCount; ++r)
            {
                for (int i = 0; i < handCount; ++i)
                {
                    UInt32 value = LutEvaluator7.Evaluate(randomHands.hands[i].Cards);
                    checksum += value;
#if DEBUG
                    VerifyHandValue(RefEvaluator.Evaluate(randomHands.hands[i].CardSet.bits, 7), value);
#endif
                }
            }

            double runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(handCount * repCount, runTime, checksum);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_RandomCards_Indexes()
        {
            int handCount = 1000000;
#if DEBUG
            int repCount = 1;
#else
            int repCount = 20;
#endif
            Console.WriteLine("Random hands: {0}, repetitions: {1}, total: {2}", handCount, repCount, handCount * repCount);

            RandomHandGenerator randomHands = new RandomHandGenerator();
            randomHands.Generate(handCount);


            // Force loading and JIT.
            UInt32 checksum = LutEvaluator7.Evaluate(0, 1, 2, 3, 4, 5, 6);
            randomHands.SetMask(7);

            Console.WriteLine("\n{0} random 7-hands generated", handCount);

            DateTime startTime = DateTime.Now;
            for (int r = 0; r < repCount; ++r)
            {
                for (int i = 0; i < handCount; ++i)
                {
                    RandomHandGenerator.Hand h = randomHands.hands[i];
                    UInt32 value = LutEvaluator7.Evaluate(h.c1, h.c2, h.c3, h.c4, h.c5, h.c6, h.c7);
                    checksum += value;
#if DEBUG
                    VerifyHandValue(RefEvaluator.Evaluate(randomHands.hands[i].CardSet.bits, 7), value);
#endif
                }
            }

            double runTime = (DateTime.Now - startTime).TotalSeconds;
            PrintResult(handCount * repCount, runTime, checksum);
        }

        #endregion

        #region Implementation

        private delegate void OnTestCombinDelegate(CardSet handCs, int[] handA);

        private void GenerateTestCombin(CardSet handCs, int[] handA, int start, int count, OnTestCombinDelegate onCombin)
        {
            if (count == handA.Length)
            {
                onCombin(handCs, handA);
                return;
            }
            for (int c = start; c <= 52 - (handA.Length - count); ++c)
            {
                CardSet newCard = StdDeck.Descriptor.CardSets[c];
                handA[count] = c;
                GenerateTestCombin(handCs | newCard, handA, c + 1, count + 1, onCombin);
            }
        }

        void VerifyHandValue(UInt32 handValExp, UInt32 handValAct)
        {
            if (handValExp != handValAct)
            {// Set a breakpoint here
            }
            Assert.AreEqual(handValExp, handValAct);
        }

        private void EvaluateAndVerify(CardSet handCs, int[] handA)
        {
            UInt32 expectedVal = RefEvaluator.Evaluate(handCs.bits, 7);
            UInt32 actualVal = LutEvaluator7.Evaluate(handA);

            // Use direct check, it is faster than NUnit assert, and for 133M repetitions it makes a difference.
            if (actualVal != expectedVal)
            {
                Assert.Fail();
            }

            actualVal = LutEvaluator7.Evaluate(handA[0], handA[1], handA[2], handA[3], handA[4], handA[5], handA[6]);

            // Use direct check, it is faster than NUnit assert, and for 133M repetitions it makes a difference.
            if (actualVal != expectedVal)
            {
                Assert.Fail();
            }

            _handTypeCounter.Count(actualVal);
        }


        private int _count = 0;
        private static HashSet<UInt32>[] _uniqHandValues = new HashSet<UInt32>[(int)HandValue.Kind._Count + 1];
        static HandTypeCounter _handTypeCounter = new HandTypeCounter();

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
        }

        void Evaluate7Cards(ref CardSet mask)
        {
            _count++;
            UInt32 handVal = CardSetEvaluator.Evaluate(ref mask);
        }


        private void PrintResult(int count, double duration, uint checksum)
        {
            Console.WriteLine("{0:###.###} s, {1:###,###,###,###} h/s, {2} comb, checksum: {3}", 
                duration, count / duration, count, checksum);
        }

        #endregion
    }
}
