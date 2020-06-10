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
    /// Unit tests for LCM. 
    /// </summary>
    [TestFixture]
    public class LCM_Test
    {
        #region Tests

        [Test]
        public void Test_Calculate()
        {
            Assert.AreEqual(1, LCM.Calculate(1, 1));
            Assert.AreEqual(2, LCM.Calculate(1, 2));
            Assert.AreEqual(37 * 39, LCM.Calculate(37, 39));
            Assert.AreEqual(60, LCM.Calculate(60, 15));
            Assert.AreEqual(37 * 3 * 5, LCM.Calculate(111, 15));
            Assert.AreEqual(1 * 3 * 5 * 7 * 11 * 13 * 17 * 23, LCM.Calculate(1 * 3 * 5 * 7 * 11 * 13 * 17, 5 * 7 * 13 * 23));

            Assert.AreEqual(1, LCM.Calculate(new int[] { 1, 1, 1 }));
            Assert.AreEqual(6, LCM.Calculate(new int[] { 1, 2, 3 }));
            Assert.AreEqual(5*7*8*9, LCM.Calculate(new int[] { 1, 2, 3, 5, 6, 7, 8, 9 }));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
