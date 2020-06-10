/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.algorithms.numbers.nunit
{
    /// <summary>
    /// Unit tests for GCD. 
    /// </summary>
    [TestFixture]
    public class GCD_Test
    {
        #region Tests

        [Test]
        public void Test_Calculate()
        {
            Assert.AreEqual(1, GCD.Calculate(1, 1));
            Assert.AreEqual(1, GCD.Calculate(1, 2));
            Assert.AreEqual(1, GCD.Calculate(37, 39));
            Assert.AreEqual(15, GCD.Calculate(60, 15));
            Assert.AreEqual(3, GCD.Calculate(111, 15));
            Assert.AreEqual(5 * 7 * 13, GCD.Calculate(1 * 3 * 5 * 7 * 11 * 13 * 17, 5 * 7 * 13 * 23));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
