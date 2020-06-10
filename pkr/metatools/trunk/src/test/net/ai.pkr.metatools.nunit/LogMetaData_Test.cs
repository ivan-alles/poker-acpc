/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.metatools.nunit
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public class LogMetaData_Test
    {
        #region Tests

        [Test]
        public void Test_Parse()
        {
            string mdString = "OnSessionBegin Name='sp-vs-raiser' Repetition='0' RngSeed='-580223114'";
            GameLogMetaData md = GameLogMetaData.Parse(mdString);
            Assert.AreEqual("OnSessionBegin", md.Name);
            Assert.AreEqual(3, md.Properties.Count);
            Assert.AreEqual("sp-vs-raiser", md.Properties["Name"]);
            Assert.AreEqual("0", md.Properties["Repetition"]);
            Assert.AreEqual("-580223114", md.Properties["RngSeed"]);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
