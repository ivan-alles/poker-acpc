/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using ai.lib.algorithms.random;
using ai.lib.algorithms;
using ai.pkr.holdem.strategy.core;

namespace ai.pkr.holdem.strategy.hs.nunit
{
    [TestFixture]
    public class HandStrength_Test
    {
        #region Tests

        [Test]
        [Explicit]
        public void Test_Caluclate_Preview()
        {
            String[] d = new String[]
                            {
                                "Ac Kc Tc Qc Jc",
                                "3c 3d Ac 7d 3h",
                                "7d 3d Ac 3c 3h",
                                "Ac 7d 3d 3c 3h",
                                "Ad Ah Ac 7d 3h",
                                "Ac Ah Qs 7d 6d",
                                "5d 4d Qs 7d 6d",
                                "8d 9d Qs 7d 6d",
                                "6c 7c 6h 2d 3s", // Top pair
                                "6c 7c 6h 2d Ts", // Mid pair
                                "6c 7c 6h Ad Ts", // Small pair
                                "6c 7c 6h 7d Kd", // 2 pair
                                "6c 7c 5h 4d Kd", // Straigh draw
                                "6c 7c Tc 2c Kd", // Flush draw
                                "6c 7c 5c 4d Kc", // Straight & flush draw
                                "Ad Ah Ac 7d 3h 5h",
                                "Ad Ah Ac 7d 3h 5h 5d",
                                "5c 5s Ac 7d 3h 5h 5d",
                                "3c 2d Kc Td 7h",
                                "3c 2d Kc Td 7h 5h Qs",
                                "Js Jd 7h 8h 9h",
                                "Js Jd 7h 8h 9h Jc",
                                "Js Jd 7h 8h 9h 6h",
                                "2c 3d 4h 5d 7c 8s 9c", // Very close to 0
                                "2c 3d 4c 5c 7c 8d 9d", // Very close to 0
                                "Ac Ad As 7s Qs 5s 5d", // Flush-like board
                                "Ac Ad Ah 7c Qd 5s 5d", // Same ranks, no flush possible
                            };
            for (int i = 0; i < d.GetLength(0); ++i)
            {
                double s = HandStrength.Calculate(_deck.GetIndexes(d[i]));
                Console.WriteLine("Hand {0} has strength {1:0.0000}", d[i], s);
            }

#if true // This runs slow
            d = new string[]
                            {
                                "Ac Ad",
                                "9s 8s",
                                "7c 2d",
                            };
            for (int i = 0; i < d.GetLength(0); ++i)
            {
                double s = HandStrength.Calculate(_deck.GetIndexes(d[i]));
                Console.WriteLine("Hand {0} has strength {1:0.0000}", d[i], s);
            }
#endif
        }

        [Test]
        public void Test_Pockets_Overall()
        {
            Console.WriteLine("Pocket hand strengths:");
            int [] indexes = new int[2];
            double weighedSumHs = 0;
            int sumCount = 0;
            for (int i = 0; i < (int)HePocketKind.__Count;  ++i)
            {
                HePocketKind pk = (HePocketKind)i;
                int[] hand = HePocket.KindToHand(pk);
                double s = HandStrength.CalculateFast(hand);
                int count = HePocket.KindToRange(pk).Length;
                Console.WriteLine("{0}  {1:0.0000} {2}", HePocket.KindToString(pk), s, count);
                weighedSumHs += s * count;
                sumCount += count;
            }
            Console.WriteLine("Weighed sum:  {0:0.0000} {1}", weighedSumHs, sumCount);
            Assert.AreEqual(1326, sumCount);
            Assert.AreEqual(1326 *0,5, weighedSumHs, "Overall result must be 0.5 (tie)");
        }

        [Test]
        [Explicit]
        public void Test_Bounds()
        {
            for (int r = 0; r < 4; ++r)
            {
                float min, max;
                HandStrength.CalculateBounds(r, out min, out max);
                Console.WriteLine("Round {0}: hand strength min {1}, max {2}", r, min, max);
            }
        }

        /// <summary>
        /// Tests calculate with some predefined hands. This is a good regression test.
        /// </summary>
        [Test]
        public void Test_Calculate_Regr()
        {
            // Values are verified by an old version of this algo
            Assert.AreEqual(0.847630, HandStrength.Calculate(_deck.GetIndexes("Ac Ad Tc Qc Jc")), 0.0000005);
            Assert.AreEqual(0.999972, HandStrength.Calculate(_deck.GetIndexes("Ac Ad As Ah 2c")), 0.0000005);
            Assert.AreEqual(0.964859, HandStrength.Calculate(_deck.GetIndexes("Ad As Ac 7d 3h")), 0.0000005);
            Assert.AreEqual(0.994699, HandStrength.Calculate(_deck.GetIndexes("Ad As Ac 3c 3h")), 0.0000005);
            Assert.AreEqual(0.945037, HandStrength.Calculate(_deck.GetIndexes("Ad Ac 3d 3c 3h")), 0.0000005);
            Assert.AreEqual(0.964861, HandStrength.Calculate(_deck.GetIndexes("Ah As Ac 7d 3h")), 0.0000005);
            Assert.AreEqual(0.816541, HandStrength.Calculate(_deck.GetIndexes("Ac Ah Qs 7d 6d")), 0.0000005);
            Assert.AreEqual(0.914273, HandStrength.Calculate(_deck.GetIndexes("Ad Ah Ac 7d 3h 5h")), 0.0000005);
            Assert.AreEqual(0.998990, HandStrength.Calculate(_deck.GetIndexes("Ad Ah Ac 7d 3h 5h 5d")), 0.0000005);
            Assert.AreEqual(0.870202, HandStrength.Calculate(_deck.GetIndexes("As Ah 5c 7d 3h 9h 5d")), 0.0000005);

            // Some obvious results. They do a good coverage because the test both win and tie.
            Assert.AreEqual(1, HandStrength.Calculate(_deck.GetIndexes("Ac Kc Qc Jc Tc")));
            Assert.AreEqual(1, HandStrength.Calculate(_deck.GetIndexes("Ac Kc Qc Jc Tc 9c")));
            Assert.AreEqual(1, HandStrength.Calculate(_deck.GetIndexes("Ac Kc Qc Jc Tc 9c 8c")));
            Assert.AreEqual(1, HandStrength.Calculate(_deck.GetIndexes("5c 5d 5h 5s 2c 3d")));
            Assert.AreEqual(1, HandStrength.Calculate(_deck.GetIndexes("5c 5d 5h 5s 2c 3d 4h")));
            Assert.AreEqual(0.5, HandStrength.Calculate(_deck.GetIndexes("7c 2d Ac Kc Qc Jc Tc")));

            // Values are verified with Poker Academy. This runs very slow.
            Assert.AreEqual(0.8520, HandStrength.Calculate(_deck.GetIndexes("Ac Ad")), 0.0005);
            Assert.AreEqual(0.5085, HandStrength.Calculate(_deck.GetIndexes("9c 8c")), 0.0005);
        }

        /// <summary>
        /// Tests CalculateFast with some hands. This is a good regression test.
        /// </summary>
        [Test]
        public void Test_CalculateFast_Regr()
        {
            string[] d = new string[]
                            {
                                //"Ac Ad Tc Qc Jc",
                                //"Ac Ad As Ah 2c",
                                //"Ad As Ac 7d 3h",
                                //"Ad As Ac 3c 3h",
                                //"Ad Ac 3d 3c 3h",
                                //"Ah As Ac 7d 3h",
                                //"Ac Ah Qs 7d 6d",
                                //"Ad Ah Ac 7d 3h 5h",
                                "Ad Ah Ac 7d 3h 5h 5d",
                                "As Ah 5c 7d 3h 9h 5d",
                            };
            float s;
            for (int i = 0; i < d.GetLength(0); ++i)
            {
                s = VerifyCalculateFast(_deck.GetIndexes(d[i]));
                Console.WriteLine("Hand {0} has strength {1:0.000000}", d[i], s);
            }

            // For preflop the calculation is very slow, therefore use samples verified with Poker Academy
            Assert.AreEqual(0.8520, HandStrength.CalculateFast(_deck.GetIndexes("Ac Ad")), 0.005);
            Assert.AreEqual(0.8520, HandStrength.CalculateFast(_deck.GetIndexes("As Ah")), 0.005);
            Assert.AreEqual(0.5085, HandStrength.CalculateFast(_deck.GetIndexes("9c 8c")), 0.005);
            Assert.AreEqual(0.5085, HandStrength.CalculateFast(_deck.GetIndexes("9s 8s")), 0.005);
            Assert.AreEqual(0.3455, HandStrength.CalculateFast(_deck.GetIndexes("7c 2d")), 0.005);
            Assert.AreEqual(0.3455, HandStrength.CalculateFast(_deck.GetIndexes("7s 2h")), 0.005);
            Assert.AreEqual(0.4165, HandStrength.CalculateFast(_deck.GetIndexes("Tc 2d")), 0.005);
            Assert.AreEqual(0.4165, HandStrength.CalculateFast(_deck.GetIndexes("Ts 2h")), 0.005);
            Assert.AreEqual(0.6705, HandStrength.CalculateFast(_deck.GetIndexes("Ac Kc")), 0.005);
            Assert.AreEqual(0.6705, HandStrength.CalculateFast(_deck.GetIndexes("Ad Kd")), 0.005);
            Assert.AreEqual(0.3225, HandStrength.CalculateFast(_deck.GetIndexes("3c 2d")), 0.005);
        }


        [Test]
        public void Test_CalculateFast_Random()
        {
            int rngSeed = (int)DateTime.Now.Ticks;
            Console.WriteLine("RNG seed {0}", rngSeed);
            // Preflop - do a few tests, they are slow
            RandomTest(rngSeed, 5, 2, 3); 
            // Postlfop
            RandomTest(rngSeed, 1000, 5, 8); 
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Caluclate()
        {
            // Skip preflop because it is very slow.
            DoBenchmark(_deck.GetIndexes("Ac Ad Td Ah Js"), 200);
            DoBenchmark(_deck.GetIndexes("Ac Ad Td Ah Js 4s"), 5000);
            DoBenchmark(_deck.GetIndexes("Ac Ad Td Ah Js 4s 9c"), 500000);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_CaluclateFast()
        {
            HandStrength.LoadPrecalculationTables();

            DoBenchmarkFast(_deck.GetIndexes("7c 2d"), 5000000);
            DoBenchmarkFast(_deck.GetIndexes("Ac Ad Td Ah Js"), 5000000);
            DoBenchmarkFast(_deck.GetIndexes("Ac Ad Td Ah Js 4s"), 5000000);
            DoBenchmarkFast(_deck.GetIndexes("Ac Ad Td Ah Js 4s 9c"), 500000);
        }

        /// <summary>
        /// To run PrecalcuateTables() manually.
        /// </summary>
        [Test]
        [Explicit]
        public void Test_PrecalcuateTables()
        {
            HandStrength.PrecalcuateTables(".", -1);
        }

        #endregion

        #region Implementation

        private void RandomTest(int rngSeed, int repCount, int handSizeBegin, int handSizeEnd)
        {
            Random rng = new Random(rngSeed);

            SequenceRng dealer = new SequenceRng(rngSeed, StdDeck.Descriptor.FullDeckIndexes);
            DateTime startTime = DateTime.Now;
            for (int r = 0; r < repCount; ++r)
            {
                // Number of cards for the pocket (2) and the board (3-5)
                // Skip preflop because it is very slow.
                int cardsCnt = rng.Next(handSizeBegin, handSizeEnd);
                dealer.Shuffle(cardsCnt);
                int[] hand = dealer.Sequence.Slice(0, cardsCnt);
                float s = VerifyCalculateFast(hand);
                //Console.WriteLine("Hand {0} has strengths {1}", _deck.GetCardNames(hand), s);
            }
            TimeSpan time = DateTime.Now - startTime;
            Console.WriteLine("{0} Repetitions done in {1:0.0} s, {2:0.0} tests/s",
                          repCount, time.TotalSeconds, repCount / time.TotalSeconds);
        }

        private void DoBenchmark(int [] hand, int repetitions)
        {
            DateTime startTime = DateTime.Now;
            double sum = 0;
            for (int r = 0; r < repetitions; ++r)
            {
                double s = HandStrength.Calculate(hand, hand.Length);
                sum += s; // To prevent optimizing away.
            }
            TimeSpan time = DateTime.Now - startTime;
            string boardName = new string[] { "", "", "PREFLOP", "", "", "FLOP", "TURN", "RIVER" }[hand.Length];
            Console.WriteLine("Hand strength on {0} calculated {1} times in {2} s, {3:0.0} val/s",
                boardName, repetitions, time.TotalSeconds, repetitions / time.TotalSeconds);
        }

        private void DoBenchmarkFast(int [] hand, int repetitions)
        {
            DateTime startTime = DateTime.Now;
            double sum = 0;
            for (int r = 0; r < repetitions; ++r)
            {
                double s = HandStrength.CalculateFast(hand);
                sum += s; // To prevent optimizing away.
            }
            TimeSpan time = DateTime.Now - startTime;
            string boardName = new string[] { "", "", "PREFLOP", "", "", "FLOP", "TURN", "RIVER" }[hand.Length];
            Console.WriteLine("Hand strength on {0} calculated {1} times in {2} s, {3:0,0} val/s",
                boardName, repetitions, time.TotalSeconds, repetitions / time.TotalSeconds);
        }

        private float VerifyCalculateFast(int [] hand)
        {
            float s1 = HandStrength.Calculate(hand);
            float s2 = HandStrength.CalculateFast(hand);
            Assert.AreEqual(s1, s2);
            CardSet pocket = StdDeck.Descriptor.GetCardSet(hand, 0, 2);
            CardSet board = StdDeck.Descriptor.GetCardSet(hand, 2, hand.Length - 2);
            float s3 = HandStrength.CalculateFast(pocket, board);
            Assert.AreEqual(s1, s3);
            return s1;
        }


        DeckDescriptor _deck = StdDeck.Descriptor;

        #endregion
    }
}
