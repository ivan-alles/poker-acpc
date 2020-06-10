/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metagame;
using ai.lib.utils;
using System.IO;
using System.Reflection;
using ai.pkr.metastrategy.vis;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for AnalyzeStrategyTree.  No verification, just let it run and check the output.
    /// </summary>
    [TestFixture]
    public class AnalyzeStrategyTree_Test
    {
        #region Tests

        [Test]
        public void Test_AnalyzeS()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            string[] strategyFiles = new string[] { "eq-KunhPoker-0-s.xml", "eq-KunhPoker-1-s.xml" };
            for(int pos = 0; pos < 2; ++pos)
            {
                string strFile = Path.Combine(_testResDir, strategyFiles[pos]);
                StrategyTree st = XmlToStrategyTree.Convert(strFile, gd.DeckDescr);
                VisStrategyTree.Show(st, Path.Combine(_outDir, string.Format("{0}-{1}.gv", gd.Name, pos)));
                AnalyzeStrategyTree an = new AnalyzeStrategyTree
                                             {
                                                 StrategyTree = st,
                                                 IsAbsolute = true, 
                                                 HeroPosition = pos,
                                                 IsVerbose = true
                                             };
                an.Analyze();
                Assert.AreEqual(15, an.LeavesCount);
                if(pos == 0)
                {
                    Assert.AreEqual(12, an.MovesCount);
                    Assert.AreEqual(5, an.ZaspMovesCount);
                    Assert.AreEqual(3, an.ZaspLeavesCount);
                    Assert.AreEqual(1, an.Statistics.Count);
                    Assert.AreEqual(5, an.Statistics[0].NZaspMovesCount);
                    Assert.AreEqual(1 + 0.33333, an.Statistics[0].SumNZaspFold, 0.00001);
                    Assert.AreEqual(0.66667 + 1 + 0.66667, an.Statistics[0].SumNZaspCall, 0.00001);
                    Assert.AreEqual(0.33333 + 1, an.Statistics[0].SumNZaspRaise, 0.00001);
                }
                else
                {
                    Assert.AreEqual(12, an.MovesCount);
                    Assert.AreEqual(4, an.ZaspMovesCount);
                    Assert.AreEqual(3, an.ZaspLeavesCount);
                    Assert.AreEqual(1, an.Statistics.Count);
                    Assert.AreEqual(6, an.Statistics[0].NZaspMovesCount);
                    Assert.AreEqual(1 + 0.66667, an.Statistics[0].SumNZaspFold, 0.00001);
                    Assert.AreEqual(0.66667 + 1 + 0.33333 + 1, an.Statistics[0].SumNZaspCall, 0.00001);
                    Assert.AreEqual(0.33333 + 1, an.Statistics[0].SumNZaspRaise, 0.00001);
                }
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "algorithms/AnalyzeStrategyTree_Test");

        #endregion
    }
}
