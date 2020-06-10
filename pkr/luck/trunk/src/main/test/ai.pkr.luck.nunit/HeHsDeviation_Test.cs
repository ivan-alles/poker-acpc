/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metagame;
using ai.pkr.holdem.strategy.hs;
using ai.lib.algorithms;

namespace ai.pkr.luck.nunit
{
    /// <summary>
    /// Unit tests for HeHsDeviation. 
    /// </summary>
    [TestFixture]
    public class HeHsDeviation_Test
    {
        #region Tests

        // Verify that the avereage HS for all preflop hands is 0.5 and
        // sum of deviations is 0.
        [Test]
        public void Test_ProofOfConcept()
        {
            ProofOfConceptParam param = new ProofOfConceptParam();
            CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, CardSet.Empty, OnCombinProofOfConcept, param);
            double avHs = param.SumHs / param.Count;
            Assert.AreEqual(0, param.SumDev, 0.0001);
            Assert.AreEqual(0.5, avHs, 0.0001);
        }

        [Test]
        public void Test_Preflop()
        {
            HeHsDeviation dev = new HeHsDeviation();

            // Generate some unlucky preflops.
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 2d"));
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("3c 2d"));
            Assert.True(dev.AccDeviation[0] < 0);
            Assert.AreEqual(0, dev.AccDeviation[1], 0.0001);
            Assert.AreEqual(0, dev.AccDeviation[2], 0.0001);
            Assert.AreEqual(0, dev.AccDeviation[3], 0.0001);
            Assert.True(dev.AvDeviation[0] < 0);
            Assert.AreEqual(0, dev.AvDeviation[1], 1e-7);
            Assert.AreEqual(0, dev.AvDeviation[2], 1e-7);
            Assert.AreEqual(0, dev.AvDeviation[3], 1e-7);
            Assert.AreEqual(new int[] { 2, 0, 0, 0 }, dev.HandCount);
            dev.Reset();

            // Generate some lucky preflops.
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("Ac Ad"));
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("Kc Ac"));
            Assert.True(dev.AccDeviation[0] > 0);
            Assert.AreEqual(0, dev.AccDeviation[1], 0.0001);
            Assert.AreEqual(0, dev.AccDeviation[2], 0.0001);
            Assert.AreEqual(0, dev.AccDeviation[3], 0.0001);
            Assert.True(dev.AvDeviation[0] > 0);
            Assert.AreEqual(0, dev.AvDeviation[1], 1e-7);
            Assert.AreEqual(0, dev.AvDeviation[2], 1e-7);
            Assert.AreEqual(0, dev.AvDeviation[3], 1e-7);
            Assert.AreEqual(new int[] { 2, 0, 0, 0 }, dev.HandCount);
            dev.Reset();

            // Make sure that the deviation for all preflop hands is 0.
            dev.Reset();
            int[] hand = new int[2];
            CardEnum.Combin(StdDeck.Descriptor, 2, hand, 0, null, 0, OnCombinProcessHand, dev);
            Assert.AreEqual(0, dev.AccDeviation[0], 0.0001);
            Assert.AreEqual(0, dev.AccDeviation[1], 0.0001);
            Assert.AreEqual(0, dev.AccDeviation[2], 0.0001);
            Assert.AreEqual(0, dev.AccDeviation[3], 0.0001);
            Assert.AreEqual(0, dev.AvDeviation[0], 1e-7);
            Assert.AreEqual(0, dev.AvDeviation[1], 1e-7);
            Assert.AreEqual(0, dev.AvDeviation[2], 1e-7);
            Assert.AreEqual(0, dev.AvDeviation[3], 1e-7);
            Assert.AreEqual(new int[] { 1326, 0, 0, 0 }, dev.HandCount);
        }

        [Test]
        public void Test_Flop()
        {
            HeHsDeviation dev = new HeHsDeviation();

            // Generate some unlucky flops.
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ah 5s 5d"));
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ad Kd Qd"));
            Assert.True(dev.AccDeviation[1] < 0);
            Assert.True(dev.AvDeviation[1] < 0);
            Assert.AreEqual(new int[] { 2, 2, 0, 0 }, dev.HandCount);
            dev.Reset();

            // Generate some lucky flops.
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ac 5c 4c"));
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ah Kc Qc"));
            Assert.True(dev.AccDeviation[1] > 0);
            Assert.True(dev.AvDeviation[1] > 0);
            Assert.AreEqual(new int[] { 2, 2, 0, 0 }, dev.HandCount);
            dev.Reset();

            // Deal all flops for a pocket and make sure the luck is zero.
            dev.Reset();
            int[] hand = new int[5];
            hand[0] = StdDeck.Descriptor.GetIndex("Ac");
            hand[1] = StdDeck.Descriptor.GetIndex("Ad");
            CardEnum.Combin(StdDeck.Descriptor, 3, hand, 2, hand, 2, OnCombinProcessHand, dev);
            Assert.AreEqual(0, dev.AccDeviation[1], 0.002);
            Assert.AreEqual(0, dev.AvDeviation[1], 1e-7);
            int hc = (int)EnumAlgos.CountCombin(50, 3);
            Assert.AreEqual(new int[] { hc, hc, 0, 0 }, dev.HandCount);

            // Deal all pockets and all flops, and make sure the luck is zero.
            // Check only average deviation, the accumulated one is too big because of low precision.
            dev.Reset();
            hand = new int[5];
            CardEnum.Combin(StdDeck.Descriptor, 2, hand, 0, null, 0, OnCombinFlop, dev);
            Assert.AreEqual(0, dev.AvDeviation[0], 0.0001);
            Assert.AreEqual(0, dev.AvDeviation[1], 0.0001);
            hc = (int)EnumAlgos.CountCombin(52, 2) * (int)EnumAlgos.CountCombin(50, 3);
            Assert.AreEqual(new int[] { hc, hc, 0, 0 }, dev.HandCount);
        }

        [Test]
        public void Test_River()
        {
            HeHsDeviation dev = new HeHsDeviation();

            // Generate some unlucky rivers.
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ac 5s 2c 3h 8h"));
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ac 5s 2h 3c 8h"));
            Assert.True(dev.AccDeviation[3] < 0);
            Assert.True(dev.AvDeviation[3] < 0);
            Assert.AreEqual(new int[] { 2, 2, 2, 2 }, dev.HandCount);
            dev.Reset();

            // Generate some lucky rivers.
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ac 5s 2c 3h 8c"));
            dev.ProcessHand(StdDeck.Descriptor.GetIndexes("7c 6c Ac 5s 2h 3c 8c"));
            Assert.AreEqual(new int[] { 2, 2, 2, 2 }, dev.HandCount);
            Assert.True(dev.AccDeviation[3] > 0);
            Assert.True(dev.AvDeviation[3] > 0);
            dev.Reset();


            // Very long running
            //// Make sure that the deviation for the river is 0.
            //dev.Reset();
            //int[] hand = new int[7];
            //CardEnum.Combin(StdDeck.Descriptor, 7, hand, 0, null, 0, OnCombinProcessHand, dev);
            //Assert.AreEqual(0, dev.AccDeviation[3], 0.0001);
            //int hc = (int)EnumAlgos.CountCombin(52, 7);
            //Assert.AreEqual(new int[] { hc, hc, hc, hc}, dev.HandCount);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class ProofOfConceptParam
        {
            public double SumHs = 0;
            public double SumDev = 0;
            public int Count = 0;
        }
        
        void OnCombinProofOfConcept(ref CardSet hand, ProofOfConceptParam param)
        {
            double hs = HandStrength.CalculateFast(StdDeck.Descriptor.GetIndexesAscending(hand).ToArray());
            param.SumHs += hs;
            param.SumDev += hs - 0.5;
            param.Count++;
        }

        void OnCombinFlop(int[] hand, HeHsDeviation dev)
        {
            CardEnum.Combin(StdDeck.Descriptor, 3, hand, 2, hand, 2, OnCombinProcessHand, dev);
        }

        void OnCombinProcessHand(int []hand, HeHsDeviation dev)
        {
            dev.ProcessHand(hand);
        }

        #endregion
    }
}
