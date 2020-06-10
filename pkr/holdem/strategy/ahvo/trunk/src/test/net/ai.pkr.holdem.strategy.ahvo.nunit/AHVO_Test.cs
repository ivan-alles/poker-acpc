/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.algorithms.random;
using ai.pkr.holdem.strategy.core;
using ai.lib.algorithms;

namespace ai.pkr.holdem.strategy.ahvo.nunit
{
    /// <summary>
    /// Unit tests for AHVO. 
    /// </summary>
    [TestFixture]
    public class AHVO_Test
    {
        #region Tests

        [Test]
        [Explicit]
        public void Test_Preview()
        {
            string[] hands = new string[]
                                 {
                                    "As 7s Qs 5s 5d", // Flush-like board
                                    "Ah 7c Qd 5s 5d", // Same ranks, no flush possible
                                    "2s 3c 6d 7h Td", // Small cards, little straight probability, no flush or more
                                    "7s 8c Td Jh Ad", // Big cards, little straight probability, no flush or more
                                    "2c 7d Qs", // Undangerous flop
                                    "2s 7s Qd", // Flush-draw flop
                                    "2s 7s Qs", // Flush flop
                                    "Ts Js Qs",  // Flush and straight flop
                                    "2c 7d 7s",  // Small-paired flop
                                    "2c Ad As",  // Big-paired flop
                                    "Ac Qs Td 2h",  // No flush turn
                                    "Ac Qc Td 2c",  // 3-flush turn
                                    "Ac Qc Tc 2c",   // 4-Flush turn
                                    "Ac Kc Qc Jc Tc"   // Highest river
                                 };
            foreach (string handS in hands)
            {
                int[] hand = StdDeck.Descriptor.GetIndexes(handS);
                float ahvo = AHVO.Calculate(hand);
                Console.WriteLine("Board: {0}  av hs: {1:0.0000}", handS, ahvo);
            }
        }

        /// <summary>
        /// Reference values are obtained by another algo.
        /// </summary>
        [Test]
        public void Test_Calculate_Simple()
        {
            Assert.AreEqual(2635.9695, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("As 7s Qs 5s 5d")), 0.0005);
            Assert.AreEqual(1600.5384, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("Ah 7c Qd 5s 5d")), 0.0005);
            Assert.AreEqual(775.4200, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("2s 3c 6d 7h Td")), 0.0005);
            Assert.AreEqual(1489.2192, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("7s 8c Td Jh Ad")), 0.0005);
            Assert.AreEqual(1264.8529, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("2c 7d Qs")), 0.0005);
            Assert.AreEqual(1335.7861, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("2s 7s Qd")), 0.0005);
            Assert.AreEqual(1751.5155, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("2s 7s Qs")), 0.0005);
            Assert.AreEqual(2225.9138, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("Ts Js Qs")), 0.0005);
            Assert.AreEqual(1981.9604, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("2c 7d 7s")), 0.0005);
            Assert.AreEqual(2532.0425, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("2c Ad As")), 0.0005);
            Assert.AreEqual(1361.4322, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("Ac Qs Td 2h")), 0.0005);
            Assert.AreEqual(1700.7279, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("Ac Qc Td 2c")), 0.0005);
            Assert.AreEqual(2831.0182, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("Ac Qc Tc 2c")), 0.0005);
            Assert.AreEqual(4823.0000, AHVO.Calculate(StdDeck.Descriptor.GetCardSet("Ac Kc Qc Jc Tc")), 0.0005);
        }

        [Test]
        [Explicit]
        public void Test_Precalculate()
        {
            AHVO.Precalculate(3);
        }

        /// <summary>
        /// Reference values are obtained by another algo.
        /// </summary>
        [Test]
        public void Test_CalculateFast_Simple()
        {
            Assert.AreEqual(2635.9695, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("As 7s Qs 5s 5d")), 0.0005);
            Assert.AreEqual(1600.5384, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("Ah 7c Qd 5s 5d")), 0.0005);
            Assert.AreEqual(775.4200, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("2s 3c 6d 7h Td")), 0.0005);
            Assert.AreEqual(1489.2192, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("7s 8c Td Jh Ad")), 0.0005);
            Assert.AreEqual(1264.8529, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("2c 7d Qs")), 0.0005);
            Assert.AreEqual(1335.7861, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("2s 7s Qd")), 0.0005);
            Assert.AreEqual(1751.5155, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("2s 7s Qs")), 0.0005);
            Assert.AreEqual(2225.9138, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("Ts Js Qs")), 0.0005);
            Assert.AreEqual(1981.9604, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("2c 7d 7s")), 0.0005);
            Assert.AreEqual(2532.0425, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("2c Ad As")), 0.0005);
            Assert.AreEqual(1361.4322, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("Ac Qs Td 2h")), 0.0005);
            Assert.AreEqual(1700.7279, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("Ac Qc Td 2c")), 0.0005);
            Assert.AreEqual(2831.0182, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("Ac Qc Tc 2c")), 0.0005);
            Assert.AreEqual(4823.0000, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet("Ac Kc Qc Jc Tc")), 0.0005);
        }

        [Test]
        public void Test_CalculateFast_Random()
        {
            CalculateFastRandomTest(1, 200);
            CalculateFastRandomTest(2, 1000);
            CalculateFastRandomTest(3, 1000);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        void CalculateFastRandomTest(int round, int repCount)
        {
            int rngSeed = (int)DateTime.Now.Ticks;
            Console.WriteLine("CalculateFast random test round {0}, rep count {1}, RNG seed {2}", round, repCount, rngSeed);

            Random rng = new Random(rngSeed);
            SequenceRng dealer = new SequenceRng(rngSeed, StdDeck.Descriptor.FullDeckIndexes);
            int boardSize = HeHelper.RoundToHandSize[round] - 2;
            for (int r = 0; r < repCount; ++r)
            {
                dealer.Shuffle(boardSize);
                double expectedAhvo = AHVO.Calculate(dealer.Sequence, boardSize);
                Assert.AreEqual(expectedAhvo, AHVO.CalculateFast(dealer.Sequence, boardSize));
                Assert.AreEqual(expectedAhvo, AHVO.CalculateFast(dealer.Sequence, 0, boardSize));
                int[] board = ContainerExtensions.Slice(dealer.Sequence, 0, boardSize);
                Assert.AreEqual(expectedAhvo, AHVO.CalculateFast(board));
                Assert.AreEqual(expectedAhvo, AHVO.CalculateFast(StdDeck.Descriptor.GetCardSet(board)));
            }
        }

        #endregion
    }
}
