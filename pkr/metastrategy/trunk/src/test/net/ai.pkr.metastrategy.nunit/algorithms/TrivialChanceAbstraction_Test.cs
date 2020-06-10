/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for TrivialAbstraction. 
    /// </summary>
    [TestFixture]
    public class TrivialChanceAbstraction_Test
    {
        #region Tests

        [Test]
        public void Test_GetAbstractCard()
        {
            TrivialChanceAbstraction tca = new TrivialChanceAbstraction();
            int[] hand = new int[] { 11, 22, 33, 44 };
            Assert.AreEqual(11, tca.GetAbstractCard(hand, 1));
            Assert.AreEqual(22, tca.GetAbstractCard(hand, 2));
            Assert.AreEqual(33, tca.GetAbstractCard(hand, 3));
            Assert.AreEqual(44, tca.GetAbstractCard(hand, 4));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
