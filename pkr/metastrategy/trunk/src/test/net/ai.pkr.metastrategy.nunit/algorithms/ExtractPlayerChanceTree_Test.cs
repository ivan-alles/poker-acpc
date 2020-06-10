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
using ai.pkr.metastrategy.vis;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for ExtractPlayerChanceTree. 
    /// </summary>
    [TestFixture]
    public unsafe class ExtractPlayerChanceTree_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));
            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            double []expectedProbabs = new double[]{1, 1.0/3, 1.0/3, 1.0/3};
            CreateAndVerifyPlayerTrees(gd, ct, expectedProbabs);
        }


        [Test]
        public void Test_LeducHe()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));
            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            double[] expectedProbabs = new double[] { 1,
                1.0 / 3, 1.0 / 15, 2.0 / 15, 2.0 / 15, 
                1.0 / 3, 2.0 / 15, 1.0 / 15, 2.0 / 15, 
                1.0 / 3, 2.0 / 15, 2.0 / 15, 1.0 / 15, };
            CreateAndVerifyPlayerTrees(gd, ct, expectedProbabs);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        private void CreateAndVerifyPlayerTrees(GameDefinition gd, ChanceTree ct, double [] expectedProbabs)
        {
            for (int pos = 0; pos < gd.MinPlayers; ++pos)
            {
                ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ct, pos);

                string fileName = string.Format("{0}-{1}.gv", gd.Name, pos);
                using (TextWriter w = new StreamWriter(File.Open(Path.Combine(_outDir, fileName), FileMode.Create)))
                {
                    VisChanceTree vis = new VisChanceTree { Output = w, CardNames = gd.DeckDescr.CardNames };
                    vis.Show(pct);
                }
                Assert.AreEqual(1, pct.PlayersCount);
                Assert.AreEqual(expectedProbabs.Length, pct.NodesCount);
                VerifyChanceTree.VerifyS(pct);
                for (int i = 0; i < expectedProbabs.Length; ++i)
                {
                    Assert.AreEqual(expectedProbabs[i], pct.Nodes[i].Probab, 0.00000000001, string.Format("Node {0}", i));
                }
            }
        }

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "algorithms/ExtractPlayerChanceTree_Test");
        
        #endregion
    }
}
