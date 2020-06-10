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
    /// Unit tests for XmlToActionTree. 
    /// </summary>
    [TestFixture]
    public unsafe class XmlToActionTree_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            string testDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
            string xmlFile = Path.Combine(testDir, "KunhPoker-at.xml"); 

            ActionTree at = XmlToActionTree.Convert(xmlFile);

            // Do some random checks
            Assert.AreEqual(1, at.Nodes[2].Position);
            Assert.AreEqual(1, at.Nodes[2].Amount);
            Assert.AreEqual(3, at.Nodes[2].ActivePlayers);
            Assert.AreEqual(-1, at.Nodes[2].Round);

            Assert.AreEqual(1, at.Nodes[4].Position);
            Assert.AreEqual(0, at.Nodes[4].Amount);
            Assert.AreEqual(3, at.Nodes[4].ActivePlayers);
            Assert.AreEqual(0, at.Nodes[4].Round);

            Assert.AreEqual(0, at.Nodes[6].Position);
            Assert.AreEqual(0, at.Nodes[6].Amount);
            Assert.AreEqual(2, at.Nodes[6].ActivePlayers);
            Assert.AreEqual(0, at.Nodes[6].Round);

            Assert.AreEqual(1, at.Nodes[10].Position);
            Assert.AreEqual(1, at.Nodes[10].Amount);
            Assert.AreEqual(3, at.Nodes[10].ActivePlayers);
            Assert.AreEqual(0, at.Nodes[10].Round);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
