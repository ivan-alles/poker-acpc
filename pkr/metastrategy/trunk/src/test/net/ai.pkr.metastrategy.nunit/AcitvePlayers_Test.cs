/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.metastrategy.nunit
{
    /// <summary>
    /// Unit tests for AcitvePlayers. 
    /// </summary>
    [TestFixture]
    public class AcitvePlayers_Test
    {
        #region Tests

        [Test]
        public void Test_GetActivePlayers()
        {
            UInt16[] ap;
            // Total: 0
            ap = ActivePlayers.Get(0, 0, 0);
            Assert.AreEqual(1, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(0, ap[0]);

            // Total: 1
            ap = ActivePlayers.Get(1, 0, 0);
            Assert.AreEqual(1, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(0, ap[0]);
            ap = ActivePlayers.Get(1, 0, 1);
            Assert.AreEqual(2, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(0, ap[0]);
            Assert.AreEqual(1, ap[1]);

            // Total: 2
            ap = ActivePlayers.Get(2, 0, 0);
            Assert.AreEqual(1, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(0, ap[0]);

            ap = ActivePlayers.Get(2, 0, 1);
            Assert.AreEqual(3, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(0, ap[0]);
            Assert.AreEqual(1, ap[1]);
            Assert.AreEqual(2, ap[2]);

            ap = ActivePlayers.Get(2, 1, 1);
            Assert.AreEqual(2, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(1, ap[0]);
            Assert.AreEqual(2, ap[1]);

            ap = ActivePlayers.Get(2, 0, 2);
            Assert.AreEqual(4, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(0, ap[0]);
            Assert.AreEqual(1, ap[1]);
            Assert.AreEqual(2, ap[2]);
            Assert.AreEqual(3, ap[3]);

            // Total: 3
            ap = ActivePlayers.Get(3, 2, 2);
            Assert.AreEqual(3, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(3, ap[0]);
            Assert.AreEqual(5, ap[1]);
            Assert.AreEqual(6, ap[2]);

            ap = ActivePlayers.Get(3, 3, 3);
            Assert.AreEqual(1, ap.Length);
            Array.Sort(ap);
            Assert.AreEqual(7, ap[0]);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
