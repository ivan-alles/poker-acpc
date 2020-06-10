/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;

namespace ai.lib.algorithms.parallel.nunit
{
    /// <summary>
    /// Unit tests for ThreadPoolLight. 
    /// </summary>
    [TestFixture]
    public class BlockingThreadPool_Test
    {
        #region Tests

        [Test]
        public void Test_FillArray()
        {
            int size = 100000000;
            if(EnvironmentExt.IsUnix())
            {
                size = 10000000;
            }
            FillArray(size, 1, 1, 2);
            FillArray(size, 1, 2, 2);
            FillArray(size, 1, 3, 2);
            FillArray(size, 1, 10, 2);
            FillArray(size, 1, 33, 2);

            FillArray(size, 2, 1, 2);
            FillArray(size, 2, 2, 1);
            FillArray(size, 2, 3, 1);
            FillArray(size, 2, 10, 2);
            FillArray(size, 2, 33, 2);

            FillArray(size, 3, 1, 2);
            FillArray(size, 3, 2, 1);
            FillArray(size, 3, 3, 1);
            FillArray(size, 3, 4, 2);
            FillArray(size, 3, 10, 2);
            FillArray(size, 3, 33, 2);

            FillArray(size, 23, 23, 2);
            FillArray(size, 23, 39, 2);
        }

        [Test]
        [Category("LongRunning")]
        [Explicit]
        public void Test_Stress()
        {
            int size = 100000000;
            if (EnvironmentExt.IsUnix())
            {
                size = 10000000;
            }
            FillArray(size, 11, 23, 100);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        /// <summary>
        /// Make segmens of different sizes, dividing array in the following proportion: 1, 2, 4, etc. 
        /// This allows to test if the thread pool uses free threads to process pending jobs.
        /// Due to different segment sizes the better performance in this test will be reached with the 
        /// greater number of segments, because idle time of threads will be lower.
        /// </summary>
        void FillArray(int size, int numThreads, int numSegments, int repetitions)
        {
            BlockingThreadPool pool = new BlockingThreadPool(numThreads);
            _array = new byte[size];

            int [] segmentSizes = new int[numSegments];
            int curSize = 0;
            double factor = size/(Math.Pow(2, numSegments)-1);
            for(int s = 0; s < numSegments-1; ++s)
            {
                segmentSizes[s] = (int)(Math.Pow(2, s)*factor);
                curSize += segmentSizes[s];
            }
            segmentSizes[numSegments - 1] = size - curSize;

            DateTime start = DateTime.Now;
            int begin;
            for (int r = 0; r < repetitions; ++r)
            {
                BlockingThreadPool.JobBase[] jobs = new BlockingThreadPool.JobBase[numSegments];
                begin = 0;
                for(int w = 0; w < numSegments; ++w)
                {
                    Parameters p = new Parameters {Start = begin, Count = segmentSizes[w], Value = w};
                    if(w == numSegments - 1)
                    {
                        p.Count = size - p.Start;
                    }
                    jobs[w] = new BlockingThreadPool.Job<Parameters>
                                   {
                                       Param1 = p,
                                       Execute = FillArrayThreadFunc
                                   };
                    begin += p.Count;
                }
                pool.ExecuteJobs(jobs);
            }

            double time = (DateTime.Now - start).TotalMilliseconds;
            Console.WriteLine("Array size: {0:0,0}, threads: {1,3}, segments: {2,3}, repeats: {3,3}, time: {4:0.0} ms, el/s: {5:#,#}",
                size, numThreads, numSegments, repetitions, time,
                (double)size * repetitions / (time * 0.001));

            // Now verify values.

            pool.Dispose();
            begin = 0;
            for(int s = 0; s < numSegments; ++s)
            {
                int end = begin + segmentSizes[s];
                for (int i = begin; i < end; ++i)
                {
                    // Check before assert, it is much faster.
                    if (s != _array[i])
                    {
                        Assert.AreEqual(s, _array[i]);
                    }
                }
                begin = end;
            }
        }


        class Parameters
        {
            public int Start;
            public int Count;
            public int Value;
        }

        void FillArrayThreadFunc(Parameters p)
        {
            for (int i = p.Start; i < p.Start + p.Count; ++i)
            {
                _array[i] = (byte)p.Value;
            }
        }

        private byte[] _array;

        #endregion
    }
}
