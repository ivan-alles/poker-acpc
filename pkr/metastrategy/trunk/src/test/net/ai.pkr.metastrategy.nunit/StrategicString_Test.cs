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
    /// Unit tests for StrategicString.
    /// </summary>
    [TestFixture]
    public class StrategicString_Test
    {
        #region Tests

        [Test]
        public void Test_ToStrategicString()
        {
            ChanceTreeNode d0 = new ChanceTreeNode { Position = 0, Card = 1 };
            ChanceTreeNode d1 = new ChanceTreeNode { Position = 1, Card = 22 };
            ActionTreeNode p0 = new ActionTreeNode { Position = 0, Amount = 5.3 };
            ActionTreeNode p1 = new ActionTreeNode { Position = 1, Amount = 3.2 };

            IStrategicAction [] actions = new IStrategicAction []{d0, d1, p0, p1};

            string s = StrategicString.ToStrategicString(actions, null);
            Assert.AreEqual("0d1 1d22 0p5.3 1p3.2", s);
        }

        [Test]
        public void Test_FromStrategicString()
        {
            string s = "0d1 1d22 0p5.3 1p3.2";
            List<IStrategicAction> actions = StrategicString.FromStrategicString(s, null);
            Assert.AreEqual(4, actions.Count);

            Assert.AreEqual("0d1", actions[0].ToStrategicString(null));
            Assert.AreEqual("1d22", actions[1].ToStrategicString(null));
            Assert.AreEqual("0p5.3", actions[2].ToStrategicString(null));
            Assert.AreEqual("1p3.2", actions[3].ToStrategicString(null));

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            s = "0dJ 1dK 2dQ";
            actions = StrategicString.FromStrategicString(s, gd.DeckDescr);
            Assert.AreEqual(3, actions.Count);

            Assert.AreEqual("0d0", actions[0].ToStrategicString(null));
            Assert.AreEqual("1d2", actions[1].ToStrategicString(null));
            Assert.AreEqual("2d1", actions[2].ToStrategicString(null));
        }


        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
