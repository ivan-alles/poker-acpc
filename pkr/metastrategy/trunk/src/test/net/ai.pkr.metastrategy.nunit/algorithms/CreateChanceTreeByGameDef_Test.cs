/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy.algorithms;
using System.Reflection;
using System.IO;
using ai.pkr.metastrategy;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for CreateChanceTreeByGameDef. 
    /// This test verifies the following:
    /// 1. Runs VerifyChanceTree
    /// 3. Game result by doing a showdown
    /// 4. Values of probabilities in leaves by using simple heuristics for each model game.
    /// </summary>
    [TestFixture]
    public unsafe class CreateChanceTreeByGameDef_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            _gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));
            ChanceTree ct = CreateChanceTreeByGameDef.Create(_gd);
            _verifyLeaf = VerifyLeaf_Kuhn;
            DoVerifyChanceTree(ct);
            Assert.AreEqual(1, ct.CalculateRoundsCount());
        }

        [Test]
        public void Test_LeducHe()
        {
            _gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));
            ChanceTree ct = CreateChanceTreeByGameDef.Create(_gd);
            _verifyLeaf = VerifyLeaf_LeducHe;
            DoVerifyChanceTree(ct);
            Assert.AreEqual(2, ct.CalculateRoundsCount());
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        void VerifyLeaf_Kuhn(ChanceTree ct, Context c)
        {
            Int64 n = c.NodeIdx;
            Assert.AreEqual(1.0 / 6, ct.Nodes[n].Probab);
        }

        void VerifyLeaf_LeducHe(ChanceTree ct, Context c)
        {
            Int64 n = c.NodeIdx;

            int[] cards = new int[3];
            cards[0] = c.Hands[0][0];
            cards[1] = c.Hands[1][0];
            cards[2] = c.Hands[0][1]; // Put shared cards once.

            string[] cardNames = _gd.DeckDescr.GetCardNamesArray(cards);

            // Kuhn leaves have only 2 distinct probabilities: 
            // 1/30 for deals where 2 cards of the same rank are present
            // 1/15 for deals where all cards are different
            double expectedProbab = 1.0 / 30;
            if (cardNames[0] != cardNames[1] && cardNames[0] != cardNames[2] && cardNames[1] != cardNames[2])
            {
                expectedProbab = 1.0 / 15;
            }

            Assert.AreEqual(expectedProbab, ct.Nodes[n].Probab);

        }

        delegate void VerifyLeaf(ChanceTree ct, Context c);

        public class Context : WalkUFTreePPContext
        {
            public bool IsLeaf;
            public List<int>[] Hands;
        }

        private void DoVerifyChanceTree(ChanceTree ct)
        {
            Assert.AreEqual(ct.PlayersCount, _gd.MinPlayers);

            try
            {
                VerifyChanceTree.VerifyS(ct);
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
            WalkUFTreePP<ChanceTree, Context> wt = new WalkUFTreePP<ChanceTree, Context>();
            wt.OnNodeBegin = OnNodeBegin;
            wt.OnNodeEnd = OnNodeEnd;
            wt.Walk(ct);
        }

        void OnNodeBegin(ChanceTree tree, Context[] stack, int depth)
        {
            Context context = stack[depth];
            Int64 n = context.NodeIdx;
            context.IsLeaf = true;
            context.Hands = new List<int>[_gd.MinPlayers];


            if (depth == 0)
            {
                for (int p = 0; p < _gd.MinPlayers; ++p)
                {
                    context.Hands[p] = new List<int>();
                }
            }
            else
            {
                stack[depth - 1].IsLeaf = false;
                for (int p = 0; p < _gd.MinPlayers; ++p)
                {
                    context.Hands[p] = new List<int>(stack[depth - 1].Hands[p]);
                }
                context.Hands[tree.Nodes[n].Position].Add(tree.Nodes[n].Card); 
            }
        }

        void OnNodeEnd(ChanceTree tree, Context[] stack, int depth)
        {
            Context context = stack[depth];
            Int64 n = context.NodeIdx;

            if (context.IsLeaf)
            {
                // Do for 2 players now
                UInt32[] ranks = new UInt32[_gd.MinPlayers];
                double[] inpot = new double[_gd.MinPlayers];
                double[] expResult = new double[_gd.MinPlayers];
                int[][] hands = new int[_gd.MinPlayers][];

                double inPotOfEachPlayer = 1.0/_gd.MinPlayers;
                for (int p = 0; p < _gd.MinPlayers; ++p)
                {
                    inpot[p] = inPotOfEachPlayer;
                    hands[p] = context.Hands[p].ToArray();
                }

                _gd.GameRules.Showdown(_gd, hands, ranks);
                Showdown.CalcualteHi(inpot, ranks, expResult, 0);

                double [] potShare = new double[_gd.MinPlayers];
                tree.Nodes[n].GetPotShare(0x3, potShare);
                for(int p = 0; p < _gd.MinPlayers; ++p)
                {
                    double actualResult = potShare[p] - inPotOfEachPlayer;
                    Assert.AreEqual(expResult[p], actualResult);
                }
                _verifyLeaf(tree, context);
            }
        }

        private const double EPSILON = 0.000000000000001;
        GameDefinition _gd;
        VerifyLeaf _verifyLeaf;

        #endregion
    }
}
