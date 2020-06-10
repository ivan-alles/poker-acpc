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
    /// Unit tests for ChanceTreeNode. 
    /// </summary>
    [TestFixture]
    public class ChanceTreeNode_Test
    {
        #region Tests

        [Test]
        public void Test_SizeOf()
        {
            int s = Marshal.SizeOf(typeof(ChanceTreeNode));
            Assert.AreEqual(18, s);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
