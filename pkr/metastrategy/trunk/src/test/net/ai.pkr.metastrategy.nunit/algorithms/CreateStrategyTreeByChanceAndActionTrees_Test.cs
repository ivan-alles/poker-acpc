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
using ai.pkr.metastrategy.nunit;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for CreateStrategyTreeByChanceAndActionTrees. 
    /// It is difficult to verify the result. Therefore for the time being just let it run
    /// and verify the number of nodes. Better verification will be done later in strategic algorithms
    /// with known game values.
    /// </summary>
    [TestFixture]
    public unsafe class CreateStrategyTreeByChanceAndActionTrees_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));
            for (int pos = 0; pos < gd.MinPlayers; ++pos)
            {
                StrategyTree st = TreeHelper.CreateStrategyTree(gd, pos);
                Assert.AreEqual(st.PlayersCount, gd.MinPlayers);
                Assert.AreEqual(30, st.NodesCount);
            }
        }



        [Test]
        public void Test_LeducHe()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));
            for (int pos = 0; pos < gd.MinPlayers; ++pos)
            {
                StrategyTree st = TreeHelper.CreateStrategyTree(gd, pos);
                Assert.AreEqual(st.PlayersCount, gd.MinPlayers);
                Assert.AreEqual(723, st.NodesCount);
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        #endregion
    }
}
