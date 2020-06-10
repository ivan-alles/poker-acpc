/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using ai.pkr.metagame;
using ai.lib.algorithms;
using ai.lib.algorithms.random;
using ai.pkr.stdpoker;

namespace ai.pkr.holdem.gamedef.nunit
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public class HoldemGameRules_Test
    {
        #region Tests

        [Test]
        public void Test_Showdown()
        {
            int seed = (int)DateTime.Now.Ticks;
            Console.WriteLine("RNG seed {0}", seed);
            SequenceRng dealer = new SequenceRng(seed, StdDeck.Descriptor.FullDeckIndexes);

            HoldemGameRules gr = new HoldemGameRules();
            int[][] hands = new int[2][];
            UInt32[] ranks = new UInt32[2];
            for (int p = 0; p < 2; ++p)
            {
                hands[p] = new int[7];
            }

            int repCount = 1000000;
            for (int r = 0; r < repCount; ++r)
            {
                dealer.Shuffle(2 + 2 + 5);
                hands[0][0] = dealer.Sequence[0];
                hands[0][1] = dealer.Sequence[1];
                hands[1][0] = dealer.Sequence[2];
                hands[1][1] = dealer.Sequence[3];
                for (int i = 0; i < 5; ++i)
                {
                    hands[0][2 + i] = hands[1][2 + i] = dealer.Sequence[4 + i];
                }
                gr.Showdown(_gd, hands, ranks);
                int actResult = -1;
                if (ranks[0] > ranks[1])
                {
                    actResult = 1;
                }
                else if (ranks[0] == ranks[1])
                {
                    actResult = 0;
                }
                CardSet h0 = _gd.DeckDescr.GetCardSet(hands[0]);
                CardSet h1 = _gd.DeckDescr.GetCardSet(hands[1]);
                UInt32 v0 = CardSetEvaluator.Evaluate(ref h0);
                UInt32 v1 = CardSetEvaluator.Evaluate(ref h1);
                int expResult = -1;
                if (v0 > v1)
                {
                    expResult = 1;
                }
                else if (v0 == v1)
                {
                    expResult = 0;
                }
                Assert.AreEqual(expResult, actResult);
            }
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Showdown()
        {
            HoldemGameRules gr = new HoldemGameRules();
            int [][] hands = new int[2][];
            UInt32[] ranks = new UInt32[2];
            for(int p = 0; p < 2; ++p)
            {
                hands[p] = new int[7];
            }
            hands[0][0] = 0;
            hands[0][1] = 1;
            hands[1][0] = 2;
            hands[1][1] = 3;
            // Force loading LUT
            UInt32 checksum = LutEvaluator7.Evaluate(0, 1, 2, 3, 4, 5, 6);
            DateTime startTime = DateTime.Now;
            int count = 0;
            for(int b0 = 4; b0 < 52 - 4; ++b0)
            {
                hands[0][2] = hands[1][2] = b0;
                for(int b1 = b0+1; b1 < 52 - 3; ++b1)
                {
                    hands[0][3] = hands[1][3] = b1;
                    for(int b2 = b1+1; b2 < 52 - 2; ++b2)
                    {
                        hands[0][4] = hands[1][4] = b2;
                        for(int b3 = b2+1; b3 < 52 - 1; ++b3)
                        {
                            hands[0][5] = hands[1][5] = b3;
                            for (int b4 = b3 + 1; b4 < 52 - 0; ++b4)
                            {
                                hands[0][6] = hands[1][6] = b4;
                                gr.Showdown(_gd, hands, ranks);
                                checksum += ranks[0] + ranks[1];
                                count++;
                            }
                        }
                    }
                }
            }
            double runTime = (DateTime.Now - startTime).TotalSeconds;
            Assert.AreEqual(EnumAlgos.CountCombin(52-4, 5), count);
            Console.WriteLine("Showdown for 2 players, {0:#,#} hands, {1:#,#} h/s, time: {2:0.000} s, checksum: {3}",
                count, count / runTime, runTime, checksum);
        }

        #endregion

        #region Implementation

        GameDefinition _gd = XmlSerializerExt.Deserialize<GameDefinition>(
            Props.Global.Expand("${bds.DataDir}/ai.pkr.holdem.gamedef/holdem-gd-fl-2.xml")); 

        #endregion
    }
}
