/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metastrategy.model_games;

namespace ai.pkr.metastrategy.model_games.nunit
{
    /// <summary>
    /// Unit tests for KuhnChanceAbstraction. 
    /// </summary>
    [TestFixture]
    public class KuhnChanceAbstraction_Test
    {
        #region Tests

        [Test]
        public void Test_GetAbstractCard()
        {
            KuhnChanceAbstraction ca = new KuhnChanceAbstraction();
            Assert.AreEqual(0, ca.GetAbstractCard(new int[]{0}, 1));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[]{1}, 1));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[]{2}, 1));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
