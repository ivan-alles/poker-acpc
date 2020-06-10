/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.algorithms.strings.nunit
{
    /// <summary>
    /// Unit tests for StringHashCode. 
    /// </summary>
    [TestFixture]
    public class StringHashCode_Test
    {
        #region Tests

        /// <summary>
        /// Generates reference hash codes. Should be started on 32-bit plattform under .NET 3.5.
        /// </summary>
        [Test]
        [Explicit]
        public void Test_Generate()
        {
            foreach (string s in _testStrings)
            {
                Console.WriteLine("{0}", s.GetHashCode());
            }
        }

        [Test]
        public void Test_Get()
        {
            for (int i = 0; i < _testStrings.Length; ++i )
            {
                Assert.AreEqual(_expectedHashes[i], StringHashCode.Get(_testStrings[i]));
            }
        }

        #endregion



        #region Benchmarks
        #endregion

        #region Implementation

        string[] _testStrings = new string[]
        {
            "",
            "\0",
            "ABBA",
            "\0ACDC",
            "\0ABBA",
            "Hello, World",
            "\x1234\x5678",
            "\x1200\x5600",
            "\x0034\x0078",
            "\x1234\x5678\x0000",
            "\x1200\x5600\x0000",
            "\x0034\x0078\x0000",
        };

        int [] _expectedHashes = new int[]
        {
            757602046,
           -842352736,
           1986449471,
           867629362,
           -1478916971,
           2085421986,
           -66934388,
           -70604384,
           -838682740,
           -1156317322,
           -1159987318,
           -1928065674
        };

        #endregion
    }
}
