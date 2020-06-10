/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.fictpl;
using ai.pkr.metastrategy;
using ai.pkr.holdem.strategy.core;
using ai.pkr.metastrategy.vis;
using System.IO;
using ai.lib.utils;
using System.Reflection;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metagame;
using ai.lib.algorithms.tree;

namespace ai.pkr.holdem.learn.nunit
{
    /// <summary>
    /// Unit tests for PreflopStrategy. 
    /// </summary>
    [TestFixture]
    public unsafe class PreflopStrategy_Test
    {
        #region Tests

        [Test]
        public void Test_CreateCt()
        {
            int oppCount = 2;
            var pockets = PocketHelper.GetAllPockets();
            //var pockets = new HePocketKind[] { HePocketKind._AA, HePocketKind._76s, HePocketKind._72o };
            var pocketDist = PocketHelper.GetProbabDistr(pockets);
            double[,] ptPeMax = MultiplayerPocketProbability.ComputePreferenceMatrixPeMax(pockets);


            string xmlAt = Props.Global.Expand("${bds.DataDir}ai.pkr.holdem.learn/nlpf-1.xml");
            ActionTree at = XmlToActionTree.Convert(xmlAt);

            string[] actionLabels = CreateActionLabels(at, 0);
            Dictionary<string, int> actionLabelToId = new Dictionary<string, int>();
            for(int i = 0; i < actionLabels.Length; ++i)
            {
                actionLabelToId.Add(actionLabels[i], i);
            }

            double[] oppDist = MultiplayerPocketProbability.Compute(oppCount, pocketDist, ptPeMax);
            ChanceTree ct = PreflopStrategy.CreateCt(pockets, oppDist);


            //GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}", "kuhn.gamedef.xml"));
            //at = CreateActionTreeByGameDef.Create(gd);
            //ct = CreateChanceTreeByGameDef.Create(gd);

            VisChanceTree.Show(ct, Path.Combine(_outDir, "ct.gv"));
            VisActionTree.Show(at, Path.Combine(_outDir, "at.gv"));

            StrategyTree[] st = SolveAndVerifyVerifySolution(at, ct, 0.001, false);

            double[,] fs = FlattenStrategy(st[0], 0, pockets.Length, actionLabelToId);

            Console.WriteLine("Result for opponent count: {0}", oppCount);
            Console.Write("{0,4}", oppCount);
            foreach(string al in actionLabels)
            {
                Console.Write("{0,20}", al);
            }
            Console.WriteLine();
            int raiseCount = 0;
            for(int c = 0; c < pockets.Length; ++c)
            {
                Console.Write("{0,4}", HePocket.KindToString(pockets[c]));
                for (int j = 0; j < actionLabels.Length; ++j)
                {
                    Console.Write("{0,20}", Math.Round(fs[c, j] * 100, 0));
                }
                if(fs[c, actionLabels.Length - 1] > 0.9)
                {
                    raiseCount += HePocket.KindToRange(pockets[c]).Length;
                }
                Console.WriteLine();
            }
            Console.WriteLine("Raise count: {0}", raiseCount);
        }

        class FlattenStrategyContext : WalkUFTreePPContext
        {
            public string ActionLabel = "";
            public double Probab = 1.0;
            public int Card = -1;
        }


        double [,] FlattenStrategy(StrategyTree st, int pos, int cardCount, Dictionary<string, int> actionLabelToId)
        {
            double[,] result = new double[cardCount,actionLabelToId.Count];
            var wt = new WalkUFTreePP<StrategyTree, FlattenStrategyContext>();
            wt.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (d > 0)
                {
                    s[d].ActionLabel = s[d - 1].ActionLabel;
                    s[d].Probab = s[d - 1].Probab;
                    s[d].Card = s[d - 1].Card;

                    if (t.Nodes[n].IsDealerAction)
                    {
                        s[d].Card = t.Nodes[n].Card;
                    }
                    else if(d > 2) // Skip blinds
                    {
                        s[d].ActionLabel = s[d].ActionLabel + "/" +
                                           String.Format("{0}p{1}", t.Nodes[n].Position, t.Nodes[n].Amount);
                        if (t.Nodes[n].IsPlayerAction(pos))
                        {
                            s[d].Probab = t.Nodes[n].Probab;
                            double pr = s[d - 1].Probab == 0 ? 0 : s[d].Probab/s[d - 1].Probab;
                            result[s[d].Card, actionLabelToId[s[d].ActionLabel]] = pr;
                        }
                    }
                }
            };
            wt.Walk(st);
            return result;
        }

        class CreateActionLabelsContext : WalkUFTreePPContext
        {
            public string Label = "";
        }


        string[] CreateActionLabels(ActionTree at, int pos)
        {
            var wt = new WalkUFTreePP<ActionTree, CreateActionLabelsContext>();
            List<string> labels = new List<string>();
            wt.OnNodeBegin = (t, s, d) =>
            {
                Int64 n = s[d].NodeIdx;
                if (d > 0)
                {
                    s[d].Label = s[d - 1].Label;
                    // Skip blinds
                    if(d > 2)
                    {
                        s[d].Label = s[d].Label + "/" + String.Format("{0}p{1}", t.Nodes[n].Position, t.Nodes[n].Amount); 
                        if(t.Nodes[n].Position == pos)
                        {
                            labels.Add(s[d].Label);
                        }
                    }
                }
            };
            wt.Walk(at);
            return labels.ToArray();
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "PreflopStrategy_Test");


        /// <summary>
        /// Solves the game by fictitious play and verifies the solution.
        /// </summary>
        /// <param name="snapshotAfter">Number of iterations to make an intermediate snapshot after. -1 for no intermediate snapshot.</param>
        /// <param name="configureSolver"></param>
        private StrategyTree[]  SolveAndVerifyVerifySolution(ActionTree at, ChanceTree ct, double epsilon, bool useLp)
        {
            if(useLp)
            {
                double[] gv;
                StrategyTree[] st = EqLp.Solve(at, ct, out gv);
                VisStrategyTree.Show(st[0], Path.Combine(_outDir, "st-0.gv"));
                VisStrategyTree.Show(st[1], Path.Combine(_outDir, "st-1.gv"));
                Console.WriteLine("LP gv: {0}, {1}", gv[0], gv[1]);
            }

            FictPlayHelper fp = new FictPlayHelper
                                    {
                                        Epsilon = epsilon,
                                        VisualizeTrees = true,
                                        BaseDir = Path.Combine(_outDir, "fp")
                                    };

            StrategyTree[] eqStrategies = fp.Solve(at, ct);
            string error;

            // Verify consistency of strategies
            for (int p = 0; p < 2; ++p)
            {
                Assert.IsTrue(VerifyAbsStrategy.Verify(eqStrategies[p], p, 1e-7, out error), string.Format("Pos {0}: {1}", p, error));
            }

            // Run VerifyEq on the computed strategies. 
            Assert.IsTrue(VerifyEq.Verify(at, ct,
                eqStrategies, 3 * epsilon, out error), error);

            return eqStrategies;
        }

        #endregion
    }
}
