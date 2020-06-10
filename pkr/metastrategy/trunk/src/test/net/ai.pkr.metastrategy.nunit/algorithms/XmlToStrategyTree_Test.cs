/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml;
using ai.lib.utils;
using System.Reflection;
using System.IO;
using ai.pkr.metagame;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for XmlToStrategyTree. 
    /// </summary>
    [TestFixture]
    public unsafe class XmlToStrategyTree_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            string testDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
            string xmlFile = Path.Combine(testDir, "eq-KunhPoker-0-s.xml"); 

            StrategyTree st = XmlToStrategyTree.Convert(xmlFile, gd.DeckDescr);

            // Do some random checks
            Assert.AreEqual(false, st.Nodes[4].IsDealerAction);
            Assert.AreEqual(0, st.Nodes[4].Position);
            Assert.AreEqual(0.666666666666667, st.Nodes[4].Probab);

            Assert.AreEqual(false, st.Nodes[6].IsDealerAction);
            Assert.AreEqual(1, st.Nodes[6].Position);
            Assert.AreEqual(1, st.Nodes[6].Amount);

            Assert.AreEqual(true, st.Nodes[12].IsDealerAction);
            Assert.AreEqual(0, st.Nodes[12].Position);
            Assert.AreEqual(1, st.Nodes[12].Card);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
