/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public unsafe class SmartPtr_Test
    {
        #region Tests

        [Test]
        public void Test_Convert()
        {
            SmartPtr ptr = UnmanagedMemory.AllocHGlobalExSmartPtr(1);
            UInt32 * pUInt32 = (UInt32*)ptr;
            Assert.IsTrue((UInt32*)ptr.Ptr.ToPointer() ==  pUInt32);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
