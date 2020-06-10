/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.utils;
using System.Reflection;
using System.IO;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for CompareStrategyTrees. 
    /// </summary>
    [TestFixture]
    public unsafe class CompareStrategyTrees_Test
    {
        #region Tests

        [Test]
        public void Test_Compare()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            string testDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
            string strFile = Path.Combine(testDir, "eq-KunhPoker-0-s.xml");


            StrategyTree st0 = XmlToStrategyTree.Convert(strFile, gd.DeckDescr);
            StrategyTree st1 = XmlToStrategyTree.Convert(strFile, gd.DeckDescr);

            CompareStrategyTrees comparer = new CompareStrategyTrees
            {
                IsVerbose = true
            };
            comparer.Compare(st0, st1);
            for (int p = 0; p < 2; ++p)
            {
                Assert.AreEqual(0, comparer.SumProbabDiff[p]);
                Assert.AreEqual(0, comparer.MaxProbabDiff[p]);
                Assert.AreEqual(0, comparer.AverageProbabDiff[p]);
                Assert.AreEqual(13, comparer.PlayerNodesCount[p]);
            }

            // Now change a probability

            Assert.IsTrue(st0.Nodes[4].Probab <= 0.9);
            Assert.IsTrue(st0.Nodes[9].Probab >= 0.1);
            st0.Nodes[4].Probab += 0.1;
            st0.Nodes[9].Probab -= 0.1;

            comparer.Compare(st0, st1);
            Assert.AreEqual(0.2, comparer.SumProbabDiff[0], 1e-8);
            Assert.AreEqual(0.1, comparer.MaxProbabDiff[0], 1e-8);
            Assert.AreEqual(0.2 / 13, comparer.AverageProbabDiff[0], 1e-8);
            Assert.AreEqual(13, comparer.PlayerNodesCount[0]);

            Assert.AreEqual(0, comparer.SumProbabDiff[1]);
            Assert.AreEqual(0, comparer.MaxProbabDiff[1]);
            Assert.AreEqual(0, comparer.AverageProbabDiff[1]);
            Assert.AreEqual(13, comparer.PlayerNodesCount[1]);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
