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
using ai.pkr.metastrategy.nunit;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for ConvertCondToAbs. 
    /// </summary>
    [TestFixture]
    public unsafe class ConvertCondToAbs_Test
    {
        #region Tests

        [Test]
        public void Test_Convert()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            StrategyTree st = TreeHelper.CreateStrategyTree(gd, 0);
            st.Nodes[4].Probab = 0.4;
            st.Nodes[9].Probab = 0.6;
            st.Nodes[7].Probab = 0.3;
            st.Nodes[8].Probab = 0.7;

            st.Nodes[13].Probab = 0.5;
            st.Nodes[18].Probab = 0.5;
            st.Nodes[16].Probab = 0.1;
            st.Nodes[17].Probab = 0.9;

            st.Nodes[22].Probab = 0.2;
            st.Nodes[27].Probab = 0.8;
            st.Nodes[25].Probab = 0.5;
            st.Nodes[26].Probab = 0.5;

            string error;
            Assert.IsTrue(VerifyCondStrategy.Verify(st, 0, out error), error);

            ConvertCondToAbs.Convert(st, 0);

            Assert.IsTrue(VerifyAbsStrategy.Verify(st, 0, 1e-7, out error), error);

            Assert.AreEqual(0.4, st.Nodes[4].Probab, 1e-7);
            Assert.AreEqual(0.6, st.Nodes[9].Probab, 1e-7);
            Assert.AreEqual(0.12, st.Nodes[7].Probab, 1e-7);
            Assert.AreEqual(0.28, st.Nodes[8].Probab, 1e-7);

            Assert.AreEqual(0.5, st.Nodes[13].Probab, 1e-7);
            Assert.AreEqual(0.5, st.Nodes[18].Probab, 1e-7);
            Assert.AreEqual(0.05, st.Nodes[16].Probab, 1e-7);
            Assert.AreEqual(0.45, st.Nodes[17].Probab, 1e-7);

            Assert.AreEqual(0.2, st.Nodes[22].Probab, 1e-7);
            Assert.AreEqual(0.8, st.Nodes[27].Probab, 1e-7);
            Assert.AreEqual(0.1, st.Nodes[25].Probab, 1e-7);
            Assert.AreEqual(0.1, st.Nodes[26].Probab, 1e-7);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
