/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.pkr.holdem.strategy.hs;
using ai.pkr.holdem.strategy.core;
using ai.lib.algorithms.random;

namespace ai.pkr.holdem.strategy.hssd.nunit
{
    /// <summary>
    /// Unit tests for HsSd. 
    /// </summary>
    [TestFixture]
    public class HsSd_Test
    {
        #region Tests

        [Test]
        public void Test_Calculate()
        {
            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;
            float[] hssd;

            // Compare with values calculated by another implementation.

            hand = dd.GetIndexes("Kh Kd");
            hssd = HsSd.Calculate(hand, 1);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.0620, hssd[1], 0.00005);

            hand = dd.GetIndexes("4s 3s");
            hssd = HsSd.Calculate(hand, 1);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.2029, hssd[1], 0.00005);

            hand = dd.GetIndexes("Qh Qc");
            hssd = HsSd.Calculate(hand, 2);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.0884, hssd[1], 0.00005);

            hand = dd.GetIndexes("Ad Kd");
            hssd = HsSd.Calculate(hand, 2);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.1963, hssd[1], 0.00005);

            hand = dd.GetIndexes("Ac Ah");
            hssd = HsSd.Calculate(hand, 3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.1060, hssd[1], 0.00005);

            hand = dd.GetIndexes("7c 6c 8c 5c Ad");
            hssd = HsSd.Calculate(hand, 2);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.2377, hssd[1], 0.00005);

            hand = dd.GetIndexes("7c 6c 8c 5c Ad");
            hssd = HsSd.Calculate(hand, 3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.3908, hssd[1], 0.00005);

            hand = dd.GetIndexes("7c 6c 8c 5c Ad Qs");
            hssd = HsSd.Calculate(hand, 3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.4038, hssd[1], 0.00005);

            hand = dd.GetIndexes("9d 9h 2c Qs Ah");
            hssd = HsSd.Calculate(hand, 2);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.0805, hssd[1], 0.00005);

            hand = dd.GetIndexes("9d 9h 2c Qs Ah");
            hssd = HsSd.Calculate(hand, 3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.1273, hssd[1], 0.00005);

            hand = dd.GetIndexes("9d 9h 2c Qs Ah Ks");
            hssd = HsSd.Calculate(hand, 3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.1125, hssd[1], 0.00005);
        }

        [Test]
        public void Test_CalculateFast()
        {
            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;
            float[] hssd;

            // Compare with values calculated by another implementation.

            hand = dd.GetIndexes("Kh Kd");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.SdPlus1);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.0620, hssd[1], 0.00005);

            hand = dd.GetIndexes("4s 3s");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.SdPlus1);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.2029, hssd[1], 0.00005);

            hand = dd.GetIndexes("Ac Ah");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.Sd3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.1060, hssd[1], 0.00005);

            hand = dd.GetIndexes("7c 6c 8c 5c Ad");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.SdPlus1);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.2377, hssd[1], 0.00005);

            hand = dd.GetIndexes("7c 6c 8c 5c Ad");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.Sd3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.3908, hssd[1], 0.00005);

            hand = dd.GetIndexes("7c 6c 8c 5c Ad Qs");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.Sd3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.4038, hssd[1], 0.00005);

            hand = dd.GetIndexes("9d 9h 2c Qs Ah");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.SdPlus1);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.0805, hssd[1], 0.00005);

            hand = dd.GetIndexes("9d 9h 2c Qs Ah");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.Sd3);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.1273, hssd[1], 0.00005);

            hand = dd.GetIndexes("9d 9h 2c Qs Ah Ks");
            hssd = HsSd.CalculateFast(hand, HsSd.SdKind.SdPlus1);
            Assert.AreEqual(HandStrength.CalculateFast(hand), hssd[0], 0.00005);
            Assert.AreEqual(0.1125, hssd[1], 0.00005);
        }

        [Test]
        public void Test_CalculateFast_Random()
        {
            CalculateFastRandomTest(0, 5);
            CalculateFastRandomTest(1, 20);
            CalculateFastRandomTest(2, 20);
            CalculateFastRandomTest(3, 20);
        }

        [Test]
        [Explicit]
        public void Test_Preview()
        {
            string[] hands = new string[]
                                 {
                                     "7c 6c 8c 5c Ad",
                                     "7c 6c 8c 5c Ad Qs",
                                     "9d 9h 2c Qs Ah",
                                     "9d 9h 2c Qs Ah Ks",
                                 };
            foreach (string handS in hands)
            {
                int[] hand = StdDeck.Descriptor.GetIndexes(handS);
                for (int r = HeHelper.HandSizeToRound[hand.Length] + 1; r <= 3; ++r)
                {
                    float[] hssd = HsSd.Calculate(hand, r);
                    Console.WriteLine("Hand: {0} round: {1} hs: {2:0.0000} sd: {3:0.0000}", handS, r, hssd[0], hssd[1]);
                }
            }
        }

        [Test]
        [Explicit]
        public void Test_HsSd_Pockets()
        {
            for (int p = 0; p < HePocket.Count; ++p)
            {
                HePocketKind pk = (HePocketKind)p;
                int[] hand = HePocket.KindToHand(pk);
                float[] hssd = HsSd.Calculate(hand, 1);
                Console.WriteLine("{0} {1:0.0000} {2:0.0000}", HePocket.KindToString(pk),
                    hssd[0], hssd[1]);
            }
        }

        [Test]
        [Category("Explicit")]
        public void Test_Precalculate()
        {
            HsSd.Precalculate(0);
            HsSd.Precalculate(1);
            HsSd.Precalculate(2);
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
            int handLength = HeHelper.RoundToHandSize[round];
            for (int r = 0; r < repCount; ++r)
            {
                dealer.Shuffle(handLength);
                HsSd.SdKind sdKind = round == 3 ? HsSd.SdKind.Sd3 : (HsSd.SdKind)rng.Next(0, 2);
                int sdRound = sdKind == HsSd.SdKind.SdPlus1 ? round + 1 : 3;
                float[] hssdFast = HsSd.CalculateFast(dealer.Sequence, handLength, sdKind);
                float[] hssd = HsSd.Calculate(dealer.Sequence, handLength, sdRound);
                Assert.AreEqual(hssd[0], hssdFast[0], 1e-6);
                Assert.AreEqual(hssd[1], hssdFast[1], 1e-6);
            }
        }

        #endregion
    }
}
