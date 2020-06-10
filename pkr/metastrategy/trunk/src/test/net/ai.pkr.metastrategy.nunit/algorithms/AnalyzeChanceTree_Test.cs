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

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for AnalyzeChanceTree.  No verification, just let it run and check the output.
    /// </summary>
    [TestFixture]
    public class AnalyzeChanceTree_Test
    {
        #region Tests

        [Test]
        public void Test_AnalyzeS()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                    Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));
            ChanceTree ct0 = CreateChanceTreeByGameDef.Create(gd);
            AnalyzeChanceTree.AnalyzeS(ct0);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
