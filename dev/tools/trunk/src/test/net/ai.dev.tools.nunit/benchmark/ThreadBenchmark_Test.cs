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
    /// Unit tests for ThreadBenchmark. 
    /// </summary>
    [TestFixture]
    public class ThreadBenchmark_Test
    {
        #region Tests

        [Test]
        public void Test_MultithreadPerformanceBest()
        {
            ThreadBenchmark b = new ThreadBenchmark {IsVerbose = true};
            int best;
            double repPerSec = b.MultithreadPerformanceBest(1, 4, 1000000, out best);
        }


        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
