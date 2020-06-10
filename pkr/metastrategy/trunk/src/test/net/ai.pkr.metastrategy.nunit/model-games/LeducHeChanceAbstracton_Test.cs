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
    /// Unit tests for LeducHeChanceAbstraction. 
    /// </summary>
    [TestFixture]
    public class LeducHeChanceAbstraction_Test
    {
        #region Tests

        [Test]
        public void Test_GetAbstractCard()
        {
            LeducHeChanceAbstraction ca = new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame);

            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 0 }, 1));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 1 }, 1));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 2 }, 1));
            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 0, 0 }, 2));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 0, 1 }, 2));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 0, 2 }, 2));
            Assert.AreEqual(3, ca.GetAbstractCard(new int[] { 1, 0 }, 2));
            Assert.AreEqual(4, ca.GetAbstractCard(new int[] { 1, 1 }, 2));
            Assert.AreEqual(5, ca.GetAbstractCard(new int[] { 1, 2 }, 2));
            Assert.AreEqual(6, ca.GetAbstractCard(new int[] { 2, 0 }, 2));
            Assert.AreEqual(7, ca.GetAbstractCard(new int[] { 2, 1 }, 2));
            Assert.AreEqual(8, ca.GetAbstractCard(new int[] { 2, 2 }, 2));

            ca = new LeducHeChanceAbstraction(LeducHeChanceAbstraction.HandRank);

            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 0 }, 1));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 1 }, 1));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 2 }, 1));
            Assert.AreEqual(3, ca.GetAbstractCard(new int[] { 0, 0 }, 2));
            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 0, 1 }, 2));
            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 0, 2 }, 2));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 1, 0 }, 2));
            Assert.AreEqual(3, ca.GetAbstractCard(new int[] { 1, 1 }, 2));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 1, 2 }, 2));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 2, 0 }, 2));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 2, 1 }, 2));
            Assert.AreEqual(3, ca.GetAbstractCard(new int[] { 2, 2 }, 2));

            ca = new LeducHeChanceAbstraction(LeducHeChanceAbstraction.Public);

            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 0 }, 1));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 1 }, 1));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 2 }, 1));
            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 0, 0 }, 2));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 0, 1 }, 2));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 0, 2 }, 2));
            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 1, 0 }, 2));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 1, 1 }, 2));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 1, 2 }, 2));
            Assert.AreEqual(0, ca.GetAbstractCard(new int[] { 2, 0 }, 2));
            Assert.AreEqual(1, ca.GetAbstractCard(new int[] { 2, 1 }, 2));
            Assert.AreEqual(2, ca.GetAbstractCard(new int[] { 2, 2 }, 2));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
