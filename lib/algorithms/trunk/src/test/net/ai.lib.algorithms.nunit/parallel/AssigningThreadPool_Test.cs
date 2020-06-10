/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;
using ai.lib.algorithms.parallel;

namespace ai.lib.algorithms.parallel.nunit
{
    /// <summary>
    /// Unit tests for AssigningThreadPool. 
    /// </summary>
    [TestFixture]
    public class AssigningThreadPool_Test
    {
        #region Tests

        [Test]
        public void Test_IncrementCounters()
        {
            IncrementCounters(10, 50, 1, 0);
            IncrementCounters(10, 50, 1, 1);
            IncrementCounters(10, 50, 2, 0);
            IncrementCounters(10, 50, 2, 1);
            IncrementCounters(10, 50, 3, 0);
            IncrementCounters(10, 50, 3, 1);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        void IncrementCounters(int numCounters, int numIncrements, int numActiveThreads, int numIdleThreads)
        {
            DateTime start = DateTime.Now;

            _counters = new int[numCounters];
            AssigningThreadPool tp = new AssigningThreadPool(numActiveThreads + numIdleThreads);
            int jobsCount = 0;
            // Repeat twice to make sure the tread pool is reusable
            for (int run = 0; run < 2; ++run)
            {
                for (int i = 0; i < numIncrements; ++i)
                {
                    for (int c = 0; c < numCounters; ++c)
                    {
                        var job = new AssigningThreadPool.Job<int, int>
                                                          {
                                                              Param1 = c,
                                                              Param2 = c,
                                                              Execute = IncCounterJob
                                                          };
                        tp.QueueJob(job, c%numActiveThreads);
                        jobsCount++;
                    }
                }
                tp.WaitAllJobs();
            }

            double time = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine("Counters: {0:0,0}, increments: {1:0,0}, active threads: {2,3}, idle threads: {3,3}, time: {4:0.000} s, jobs/s: {5:#,#}",
                numCounters, numIncrements, numActiveThreads, numIdleThreads, time, jobsCount / time);
            tp.Dispose();
            // Verify result
            for (int c = 0; c < numCounters; ++c)
            {
                Assert.AreEqual(2*c * numIncrements, _counters[c]);
            }
        }

        void IncCounterJob(int id, int value)
        {
            Thread.Sleep(1); // To let the job last some time.
            _counters[id] += value;
        }

        private int[] _counters;
        #endregion
    }
}
