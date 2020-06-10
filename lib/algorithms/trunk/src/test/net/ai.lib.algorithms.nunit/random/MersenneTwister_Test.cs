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
using ai.lib.algorithms.random;
using ai.lib.algorithms.random.nunit;

namespace ai.lib.algorithms.random.nunit
{
    /// <summary>
    /// Unit tests for MersenneTwister. 
    /// </summary>
    [TestFixture]
    public class MersenneTwister_Test
    {
        #region Tests

        /// <summary>
        /// Generate files for diehard test.
        /// </summary>
        [Test]
        [Explicit]
        public void Test_Diehard()
        {
            Random r = new MersenneTwister();
            TestRandom test = new TestRandom(r, _outDir);
            test.CreateFileForDiehard(12*1000000);
        }

        [Test]
        [Explicit]
        public void Test_Reciprocal()
        {
            double reciprocal = 1.0/4294967295.000001;
            UInt32 mersenneNumber = 0xFFFFFFFF;
            double d = reciprocal * mersenneNumber;
            Assert.AreNotEqual(1.0, d);
        }
        

        /// <summary>
        /// Make sure the same result is generated for the same seed.
        /// </summary>
        [Test]
        public void Test_SameSeed()
        {
            int seed = 123;
            MersenneTwister rng1;
            MersenneTwister rng2;

            rng1 = new MersenneTwister(seed);
            rng2 = new MersenneTwister(seed);

            for (int i = 0; i < 100; ++i)
            {
                Assert.AreEqual(rng1.Next(), rng2.Next());
            }
        }


        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Becnhmark_NextDouble()
        {
            Random r = new MersenneTwister();
            TestRandom test = new TestRandom(r, _outDir);
            test.BenchmarkNextDouble(20 * 1000000);
        }

        #endregion

        #region Implementation

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "random/MersenneTwister_Test");


        #endregion
    }
}
