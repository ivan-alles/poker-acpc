/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metastrategy.model_games;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metastrategy.vis;
using System.IO;
using System.Reflection;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for CreateChanceTreeByAbstraction. 
    /// At the moment veryfies only the abstraction of Kuhn, because it can be verified easily.
    /// The other abstractions are verified indirectly, e.g. by comparing with Open CFR results.
    /// This ensures a good test coverage.
    /// </summary>
    [TestFixture]
    public unsafe class CreateChanceTreeByAbstraction_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            IChanceAbstraction[] abstractions = new IChanceAbstraction []{ new KuhnChanceAbstraction(), new KuhnChanceAbstraction() };

            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            ChanceTree act = CreateChanceTreeByAbstraction.CreateS(gd, abstractions);

            VerifyChanceTree.VerifyS(act);

            VisChanceTree.Show(act, Path.Combine(_outDir, gd.Name + "-abct.gv"));

            UInt16[] activePlayersOne = ActivePlayers.Get(gd.MinPlayers, 1, 1);
            UInt16[] activePlayersAll = ActivePlayers.Get(gd.MinPlayers, 1, gd.MinPlayers);
            int maxDepth = gd.RoundsCount * gd.MinPlayers;

            Assert.AreEqual(act.PlayersCount, gd.MinPlayers);
            Assert.AreEqual(ct.NodesCount, act.NodesCount);
            double[] expPotShares = new double[gd.MinPlayers];
            double[] actPotShares = new double[gd.MinPlayers];
            for (int i = 0; i < ct.NodesCount; ++i)
            {
                Assert.AreEqual(ct.Nodes[i].Position, act.Nodes[i].Position);
                Assert.AreEqual(ct.Nodes[i].Card, act.Nodes[i].Card); // Kuhn abstraction has the same card
                Assert.AreEqual(ct.Nodes[i].Probab, act.Nodes[i].Probab);
                if (i == 0)
                {
                    continue;
                }
                UInt16[] activePlayers = ct.GetDepth(i) == maxDepth ? activePlayersAll : activePlayersOne;
                foreach (UInt16 ap in activePlayers)
                {
                    ct.Nodes[i].GetPotShare(ap, expPotShares);
                    act.Nodes[i].GetPotShare(ap, actPotShares);
                    Assert.AreEqual(expPotShares, actPotShares, String.Format("Node: {0}, ap: {1}", i, ap));
                }
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "algorithms/CreateChanceTreeByAbstraction_Test");

        #endregion
    }
}
