/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metastrategy.nunit
{
    /// <summary>
    /// Unit tests for StrategyTree. 
    /// </summary>
    [TestFixture]
    public class StrategyTree_Test
    {
        #region Tests

        [Test]
        public void Test_FindNode()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));
            StrategyTree st = TreeHelper.CreateStrategyTree(gd, 0);
            Assert.AreEqual(3, st.FindNode("0dJ", gd.DeckDescr));
            Assert.AreEqual(11, st.FindNode("0dJ 0p1 1p1", gd.DeckDescr));
            Assert.AreEqual(13, st.FindNode("0dQ 0p0", gd.DeckDescr));
            Assert.AreEqual(16, st.FindNode("0d1 0p0 1p1 0p0", null));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
