/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ai.lib.algorithms;
using ai.lib.algorithms.parallel;

namespace ai.dev.tools.benchmark
{
    /// <summary>
    /// Measures system performance for multithreading execution.
    /// </summary>
    public class ThreadBenchmark
    {
        class Work
        {
            public Work(UInt64 repeatCount)
            {
                _repeatCount = repeatCount;
            }

            public void Execute()
            {
                // Ignore parameters as we already stored them.

                // Compute Fibonacci numbers and store them to array.
                _array[0] = 1;
                _array[1] = 1;
                int i = 2;
                for(UInt64 r = 0; r < _repeatCount; ++r, ++i)
                {
                    if (i >= _array.Length)
                    {
                        i = 2;
                    }
                    double f1 = _array[i-2];
                    double f2 = _array[i - 1];
                    double f = f1 + f2;
                    _array[i] = f;

                }
            }

            private UInt64 _repeatCount;
            private Double[] _array = new double[1000000];
        }

        /// <summary>
        /// If true, prints messages to console.
        /// </summary>
        public bool IsVerbose
        {
            set;
            get;
        }

        /// <summary>
        /// Run a number of threads doing some repetitive operations. 
        /// Currently the job is to calculating Fibonacci numbers and storing them in an array.
        /// At the end the number of operations per seconds for all threads actually done is returned.
        /// This allows to estimate the optimal number of threads to perform some calcluation task.
        /// </summary>
        /// <param name="threadsCount"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public double MultithreadPerformance(int threadsCount, UInt64 repeatCount)
        {
            BlockingThreadPool threadPool = new BlockingThreadPool(threadsCount);
            BlockingThreadPool.Job[] works = new BlockingThreadPool.Job[threadsCount];
            for(int w = 0; w < threadsCount; ++w)
            {
                Work work = new Work(repeatCount);
                works[w] = new BlockingThreadPool.Job
                               {
                                   DoDelegate = work.Execute
                               };
            }

            DateTime startTime = DateTime.Now;
            threadPool.ExecuteJobs(works);

            double workTime = (DateTime.Now - startTime).TotalSeconds;
            double repPerSec = ((double)threadsCount * repeatCount) / workTime;
            Console.WriteLine(
                "MultithreadPerformance treads: {0}, repeatitions/tread: {1:0,0}, work time: {2:0.000} s, rep/s: {3:0,0}", 
                threadsCount, repeatCount, workTime, repPerSec);
            return repPerSec;
        }

        /// <summary>
        /// Finds the optimal thread count callining repeatedly MultithreadPerformance() with
        /// the number of threads in the given range with step 1.
        /// </summary>
        public double MultithreadPerformanceBest(int threadsCountBegin, int threadsCountEnd, UInt64 repeatCount, out int bestThreadCount)
        {
            Console.WriteLine("MultithreadPerformanceBest started: tread count begin: {0}, end: {1}", threadsCountBegin, threadsCountEnd);
            double best = double.MinValue;
            bestThreadCount = -1;
            for (int threadCount = threadsCountBegin; threadCount <= threadsCountEnd; ++threadCount)
            {
                double cur = MultithreadPerformance(threadCount, repeatCount);
                if (cur > best)
                {
                    best = cur;
                    bestThreadCount = threadCount;
                }
            }
            Console.WriteLine("MultithreadPerformanceBest finished: tread count begin: {0}, end: {1}, best result: {2:0,0} rep/s for thread count {3}",
                threadsCountBegin, threadsCountEnd, best, bestThreadCount);
            return best;
        }

    }
}
