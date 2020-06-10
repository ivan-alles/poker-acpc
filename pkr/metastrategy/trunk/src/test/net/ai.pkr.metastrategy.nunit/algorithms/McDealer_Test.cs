/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.pkr.metastrategy.algorithms;
using ai.lib.algorithms;
using ai.lib.utils;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for McHandGen. 
    /// </summary>
    [TestFixture]
    public class McDealer_Test
    {
        #region Tests

        [Test]
        public void Test_DealNext()
        {
            GameDefinition gdKuhn = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            Verify(gdKuhn, 1, 100);
            Verify(gdKuhn, 2, 100);

            GameDefinition gdLeducHe = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));

            Verify(gdLeducHe, 1, 100);
            Verify(gdLeducHe, 2, 100);

            GameDefinition gdTestHe = CreateHeGamedef();

            Verify(gdTestHe, 1, 100);
            Verify(gdTestHe, 2, 100);
            Verify(gdTestHe, 3, 100);
        }

        #endregion

        #region Benchmarks


        [Test]
        [Category("Benchmark")]
        public void Benchmark_DealNext()
        {
            GameDefinition gdTestHe = CreateHeGamedef();

            Benchmark(gdTestHe, 1, 2000000);
            Benchmark(gdTestHe, 2, 2000000);
            Benchmark(gdTestHe, 3, 2000000);
        }

        #endregion

        #region Implementation

        GameDefinition CreateHeGamedef()
        {
            GameDefinition gd = new GameDefinition { Name = "TestHE" };
            gd.RoundsCount = 4;
            gd.PrivateCardsCount = new int[] { 2, 0, 0, 0 };
            gd.PublicCardsCount = new int[] { 0, 0, 0, 0 };
            gd.SharedCardsCount = new int[] { 0, 3, 1, 1 };
            gd.DeckDescr = StdDeck.Descriptor;
            return gd;
        }

        /// <summary>
        /// Generates some random hands and verifies the result.
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="repCount"></param>
        void Verify(GameDefinition gd, int playersCount, int repCount)
        {
            int seed = (int)DateTime.Now.Ticks;
            Console.WriteLine("Game: {0}, players: {1}, RNG seed: {2}", gd.Name, playersCount, seed);
            McDealer mcDealer = new McDealer(gd, seed);

            List<bool> isSharedDeal = new List<bool>();
            int totalCardsCount = 0;
            for (int r = 0; r < gd.RoundsCount; ++r)
            {
                for (int i = 0; i < gd.PublicCardsCount[r] + gd.PrivateCardsCount[r]; ++i)
                {
                    isSharedDeal.Add(false);
                }
                totalCardsCount += (gd.PublicCardsCount[r] + gd.PrivateCardsCount[r]) * playersCount;

                for (int i = 0; i < gd.SharedCardsCount[r]; ++i)
                {
                    isSharedDeal.Add(true);
                }
                totalCardsCount += gd.SharedCardsCount[r];
            }
            Assert.AreEqual(isSharedDeal.Count, mcDealer.HandSize);
            int [][] hands = new int[playersCount][].Fill(i => new int[isSharedDeal.Count]);
            for(int rep = 0; rep < repCount; ++rep)
            {
                mcDealer.NextDeal(hands);
                HashSet<int> distinctCards = new HashSet<int>();
                for (int d = 0; d < isSharedDeal.Count; ++d)
                {
                    if (isSharedDeal[d])
                    {
                        // Make sure all players has the same card.
                        for (int p = 0; p < playersCount; ++p)
                        {
                            Assert.AreEqual(hands[0][d], hands[p][d]);
                        }
                        Assert.IsFalse(distinctCards.Contains(hands[0][d]));
                        distinctCards.Add(hands[0][d]);
                    }
                    else
                    {
                        // Add cards of each player to the set. If multiple players have
                        // this card or it was dealt before, we will get an error.
                        for (int p = 0; p < playersCount; ++p)
                        {
                            Assert.IsFalse(distinctCards.Contains(hands[p][d]));
                            distinctCards.Add(hands[p][d]);
                        }
                    }
                }
                Assert.AreEqual(totalCardsCount, distinctCards.Count);
            }
        }

        void Benchmark(GameDefinition gd, int playersCount, int repCount)
        {
            int seed = (int)DateTime.Now.Ticks;
            McDealer mcDealer = new McDealer(gd, seed);

            int[][] hands = new int[playersCount][].Fill(i => new int[mcDealer.HandSize]);
            int checksum = 0;

            DateTime startTime = DateTime.Now;
            for (int rep = 0; rep < repCount; ++rep)
            {
                mcDealer.NextDeal(hands);
                checksum += hands[0][0];
            }
            double time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine("Game: {0}, players: {1}, repetitions: {2:0,0}, time: {3:0.000} s, {4:0,0} deals/s", 
                gd.Name, playersCount, repCount, time, repCount / time);
        }

        #endregion
    }
}
