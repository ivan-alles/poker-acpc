/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.lib.algorithms;
using ai.pkr.stdpoker;
using ai.pkr.holdem.strategy.core;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.holdem.strategy.hs;

namespace ai.pkr.holdem.strategy.ca.nunit
{
    /// <summary>
    /// Testbed for experimental algos. 
    /// </summary>
    [TestFixture]
    public unsafe class Experiments_Test
    {
        #region Tests

        [Test]
        [Explicit]
        public void Test_HsSd_Pockets()
        {
            for (int p = 0; p < HePocket.Count; ++p)
            {
                HePocketKind pk = (HePocketKind)p;
                int[] hand = HePocket.KindToHand(pk);
                double[] hssd = CalculateHsSd(hand, 2, 6);
                Console.WriteLine("{0} {1:0.0000} {2:0.0000}", HePocket.KindToString(pk),
                    hssd[0], hssd[1]);
            }
        }

        [Test]
        [Explicit]
        public void Test_HsSd_Hands()
        {
            string[] hands = new string[]
                                 {
                                     "7c 6c 8c 5c Ad",
                                     "7c 6c 8c 5c Ad Qs",
                                     "9d 9h 2c Qs Ah",
                                     "9d 9h 2c Qs Ah Ks",
                                 };
            foreach(string handS in hands)
            {
                int[] hand = StdDeck.Descriptor.GetIndexes(handS);
                for (int r = HeHelper.HandSizeToRound[hand.Length]+1; r <= 3; ++r)
                {
                    double[] hssd = CalculateHsSd(hand, hand.Length, HeHelper.RoundToHandSize[r]);
                    Console.WriteLine("Hand: {0} round: {1} hs: {2:0.0000} sd: {3:0.0000}", handS, r, hssd[0], hssd[1]);
                }
            }
        }

        [Test]
        [Explicit]
        public void Test_AvHVO_Hands()
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
                double avHs = CalculateAverageHVO(hand);
                Console.WriteLine("Board: {0}  av hs: {1:0.0000}", handS, avHs);
            }
        }

        [Test]
        [Explicit]
        public void Test_HsDistribution()
        {
            string[] boards = new string[]
                                 {
                                    "As 7s Qs 5s 5d", // Flush-like board
                                    "Ah 7c Qd 5s 5d", // Same ranks, no flush possible
#if false
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
#endif
                                 };
            foreach (string board in boards)
            {
                StdDeck.Descriptor.GetIndexes(board);
                List<HandDistrEntry> dist = CalculateHsDistribution(board);
                Console.WriteLine("HS distirbution for board: {0}", board);
                for (int i = 0; i < dist.Count; ++i)
                {
                    Console.WriteLine("{0} {1:0.0000}", dist[i].Pocket, dist[i].Hs);
                }
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation


        class Params
        {
            public int[] Hand;
            public int HandLength;
            public int Count;
            public float ExpHs;
            public double SumHs;
            public double SumDiff;
        }

        public double[] CalculateHsSd(int[] hand, int handLength, int finalHandSize)
        {
            Debug.Assert(handLength >= 2 && handLength <= 7);

            CardSet cs = StdDeck.Descriptor.GetCardSet(hand, 0, handLength);
            Params p = new Params{ Hand = new int [7], HandLength = handLength};
            for (int i = 0; i < handLength; ++i)
            {
                p.Hand[i] = hand[i];
            }

            p.ExpHs = HandStrength.CalculateFast(hand, handLength);
            CardEnum.Combin<Params>(StdDeck.Descriptor, finalHandSize - handLength, CardSet.Empty, cs, OnDeal, p);
            Assert.AreEqual(p.ExpHs, p.SumHs / p.Count, 0.00001);
            double[] hssd = new double[2];
            hssd[0] = p.ExpHs;
            hssd[1] = Math.Sqrt(p.SumDiff / p.Count);
            return hssd;
        }

        void OnDeal(ref CardSet cs, Params p)
        {
            int[] dealt = StdDeck.Descriptor.GetIndexesAscending(cs).ToArray();
            dealt.CopyTo(p.Hand, p.HandLength);

            double hs = HandStrength.CalculateFast(p.Hand, p.HandLength + dealt.Length);
            double d = hs - p.ExpHs;
            p.SumDiff += d * d;
            p.SumHs += hs;
            p.Count++;
        }

        class CalculateAverageHsParam
        {
            public double Sum = 0;
            public int Count = 0;
        }

        double CalculateAverageHVO(int[] board)
        {
            CardSet boardCs = StdDeck.Descriptor.GetCardSet(board);
            CalculateAverageHsParam param = new CalculateAverageHsParam();
            int toDealCount = 7 - board.Length;
            CardEnum.Combin(StdDeck.Descriptor, toDealCount, boardCs, CardSet.Empty, OnPocket, param);
            Assert.AreEqual(EnumAlgos.CountCombin(52 - board.Length, toDealCount), param.Count);
            return param.Sum/param.Count;
        }

        void OnPocket(ref CardSet hand, CalculateAverageHsParam param)
        {
            uint handValue = CardSetEvaluator.Evaluate(ref hand);
            int ordinal = HandValueToOrdinal.GetOrdinal7(handValue);
            param.Sum += ordinal;
            param.Count++;
        }

        class HandDistrEntry: IComparable<HandDistrEntry>
        {
            public CardSet Pocket;
            public float Hs;

            public int CompareTo(HandDistrEntry other)
            {
                return -Hs.CompareTo(other.Hs);
            }
        }

        class HandDistrParams
        {
            public List<HandDistrEntry> Distr;
            public CardSet Board;
        }

        private List<HandDistrEntry> CalculateHsDistribution(string boardS)
        {
            CardSet board = StdDeck.Descriptor.GetCardSet(boardS);
            HandDistrParams param = new HandDistrParams {Board = board, Distr = new List<HandDistrEntry>()};
            CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, board, OnPocket, param);
            param.Distr.Sort();
            return param.Distr;
        }

        void OnPocket(ref CardSet pocket, HandDistrParams param)
        {
            param.Distr.Add(new HandDistrEntry {Pocket = pocket, Hs = HandStrength.CalculateFast(pocket, param.Board)});
        }

        #endregion
    }
}
