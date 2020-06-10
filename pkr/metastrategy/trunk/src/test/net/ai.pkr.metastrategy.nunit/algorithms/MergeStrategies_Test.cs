/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.pkr.metastrategy.algorithms;
using ai.lib.utils;
using ai.lib.algorithms;
using System.IO;
using System.Reflection;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for MergeStrategies. 
    /// Merge existing strategies into one, calculates the game value and compares with the original game value.
    /// </summary>
    [TestFixture]
    public class MergeStrategies_Test
    {
        #region Tests

        [Test]
        public void Test_Leduc()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));

            string[] strategyFilesRel = new string[] { "eq-LeducHe-0-s.xml", "eq-LeducHe-1-s.xml" };
            string[] StrategyFilesAbs = new string[gd.MinPlayers].Fill(i => Path.Combine(_testResDir, strategyFilesRel[i]));

            StrategyTree[]  strategiesOrig = new StrategyTree[gd.MinPlayers].Fill(i =>
                XmlToStrategyTree.Convert(StrategyFilesAbs[i], gd.DeckDescr));

            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            ActionTree at = CreateActionTreeByGameDef.Create(gd);

            GameValue gv1 = new GameValue{ ActionTree = at, ChanceTree = ct, Strategies = strategiesOrig };
            gv1.Solve();

            for (int p = 0; p < gd.MinPlayers; ++p)
            {
                StrategyTree[] strategiesMerged = new StrategyTree[gd.MinPlayers].Fill(i =>
                    XmlToStrategyTree.Convert(StrategyFilesAbs[i], gd.DeckDescr));

                // Merge strategy to the stategy at postion p.
                for (int pSrc = 0; pSrc < gd.MinPlayers; ++pSrc)
                {
                    if (p == pSrc) continue;
                    MergeStrategies.Merge(strategiesMerged[p], strategiesMerged[pSrc], pSrc);
                }
                // Copy strategy at position p to the other positions
                for (int pCopy = 0; pCopy < gd.MinPlayers; ++pCopy)
                {
                    strategiesMerged[pCopy] = strategiesMerged[p];
                }

                GameValue gv2 = new GameValue { ActionTree = at, ChanceTree = ct, Strategies = strategiesMerged };
                gv2.Solve();
                Assert.AreEqual(gv1.Values, gv2.Values);
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "algorithms/MergeStrategies_Test");

        #endregion
    }
}
