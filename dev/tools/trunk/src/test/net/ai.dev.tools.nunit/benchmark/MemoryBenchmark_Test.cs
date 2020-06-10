/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.dev.tools.benchmark;

namespace ai.dev.tools.nunit.benchmark
{
    /// <summary>
    /// Unit tests for MemoryBenchmark. 
    /// </summary>
    [TestFixture]
    public class MemoryBenchmark_Test
    {
        #region Tests

        /// <summary>
        /// Runs exhaustive memory test. Set it to explicit, because it puts the whole system under stress,
        /// this is not what we want in a UT.
        /// </summary>
        [Test]
        [Explicit]
        public void Test_AllocateArrayExhaustiveBest()
        {
            MemoryBenchmark b = new MemoryBenchmark
                                    {
                                        IsVerbose = true
                                    };

            int bestElCount;
            b.AllocateArrayExhaustiveBest<UInt64>(128, 512, out bestElCount);

            // Nothing to verify here, just see if it runs.
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
