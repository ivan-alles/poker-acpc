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
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public class ThreadSyncBenchmark_Test
    {
        #region Tests


        [Test]
        public void Test_SumValuesNoSync()
        {
            ThreadSyncBenchmark b = new ThreadSyncBenchmark();
            int repetitions = 5000000;
            int threadCount = 2;
            float result = b.SumValuesNoSync(threadCount, repetitions);
            Console.WriteLine("Threads: {0}, repetitions: {1:#,#}, time: {2:0.0000} s, {3:#,#} r/s, result: {4}", threadCount,
                              repetitions,
                              b.RunTime, (double)threadCount * repetitions / b.RunTime, result);
        }

        [Test]
        public void Test_SumValuesInterlocked()
        {
            ThreadSyncBenchmark b = new ThreadSyncBenchmark();
            int repetitions = 5000000;
            int threadCount = 2;
            float result = b.SumValuesInterlocked(threadCount, repetitions);
            Console.WriteLine("Threads: {0}, repetitions: {1:#,#}, time: {2:0.0000} s, {3:#,#} r/s, result: {4}", threadCount,
                              repetitions,
                              b.RunTime, (double) threadCount*repetitions/b.RunTime, result);
            Assert.AreEqual(threadCount * repetitions, result);
        }

        [Test]
        public void Test_SumValuesLock()
        {
            ThreadSyncBenchmark b = new ThreadSyncBenchmark();
            int repetitions = 5000000;
            int threadCount = 2;
            float result = b.SumValuesLock(threadCount, repetitions);
            Console.WriteLine("Threads: {0}, repetitions: {1:#,#}, time: {2:0.0000} s, {3:#,#} r/s, result: {4}", threadCount,
                              repetitions,
                              b.RunTime, (double)threadCount * repetitions / b.RunTime, result);
            Assert.AreEqual(threadCount * repetitions, result);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
