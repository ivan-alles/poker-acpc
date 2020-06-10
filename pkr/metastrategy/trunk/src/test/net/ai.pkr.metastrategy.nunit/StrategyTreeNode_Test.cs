/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace ai.pkr.metastrategy.nunit
{
    /// <summary>
    /// Unit tests for StrategyTreeNode. 
    /// </summary>
    [TestFixture]
    public class StrategyTreeNode_Test
    {
        #region Tests

        [Test]
        public void Test_SizeOf()
        {
            int s = Marshal.SizeOf(typeof(StrategyTreeNode));
            Assert.AreEqual(12, s);
        }

        /// <summary>
        /// Check if property accessor encoding works correctly.
        /// </summary>
        [Test]
        public void Test_Properties()
        {
            StrategyTreeNode n = new StrategyTreeNode();

            n.IsDealerAction = false;
            Assert.IsFalse(n.IsDealerAction);

            for (int p = 0; p < 7; ++p)
            {
                n.Position = p;
                Assert.AreEqual(p, n.Position);
            }
            n.Amount = 0;
            Assert.AreEqual(0, n.Amount);
            n.Amount = 2000;
            Assert.AreEqual(2000, n.Amount);

            for (double a = 0; a <= 2000.0; a += 0.54321)
            {
                double amount = Math.Round(a, 5);
                n.Amount = amount;
                Assert.AreEqual(amount, n.Amount);
            }
            n.Probab = 0;
            Assert.AreEqual(0, n.Probab);
            n.Probab = 0.123;
            Assert.AreEqual(0.123, n.Probab);
            n.Probab = 1;
            Assert.AreEqual(1, n.Probab);

            n.IsDealerAction = true;
            Assert.IsTrue(n.IsDealerAction);

            for (int p = 0; p < 7; ++p)
            {
                n.Position = p;
                Assert.AreEqual(p, n.Position);
            }
            for (int c = 0; c <= 1000; ++c)
            {
                n.Card = c;
                Assert.AreEqual(c, n.Card);
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
