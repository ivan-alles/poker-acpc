/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MathNet.Numerics.RandomSources;
using System.IO;
using System.Reflection;
using ai.lib.utils;
using ai.lib.algorithms.random.nunit;

namespace ai.lib.algorithms.random.nunit
{
    /// <summary>
    /// Unit tests for System.Random. 
    /// </summary>
    [TestFixture]
    public class SystemRandom_Test
    {
        #region Tests

        /// <summary>
        /// Generate files for diehard test.
        /// </summary>
        [Test]
        [Explicit]
        public void Test_Diehard()
        {
            Random r = new System.Random();
            TestRandom test = new TestRandom(r, _outDir);
            test.CreateFileForDiehard(12*1000000);
        }


        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Becnhmark_NextDouble()
        {
            Random r = new System.Random();
            TestRandom test = new TestRandom(r, _outDir);
            test.BenchmarkNextDouble(20 * 1000000);
        }

        #endregion

        #region Implementation

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "random/SystemRandom_Test");


        #endregion
    }
}
