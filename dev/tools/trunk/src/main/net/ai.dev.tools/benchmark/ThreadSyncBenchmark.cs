/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ai.lib.algorithms.parallel;

namespace ai.dev.tools.benchmark
{
    /// <summary>
    /// Benchmarks various method to synchronized threads so that they can be compared to each other.
    /// Also does non-sychronized operaition (result is wrong on multi-CPU machines) to compare running time.
    /// </summary>
    public class ThreadSyncBenchmark
    {
        // Field totalValue contains a running total that can be updated
        // by multiple threads. It must be protected from unsynchronized 
        // access.
        private float _totalValue = 0.0F;

        /// <summary>
        /// Does interlocked summing. Using of static function and passing variable by ref is
        /// faster than access to a field.
        /// </summary>
        static void AddToTotalInterlocked(ref float totalValue, float addend)
        {
            float initialValue;
            do
            {
                // Save the current running total in a local variable.
                initialValue = totalValue;

                // CompareExchange compares totalValue to initialValue. If
                // they are not equal, then another thread has updated the
                // running total since this loop started. CompareExchange
                // does not update totalValue. CompareExchange returns the
                // contents of totalValue, which do not equal initialValue,
                // so the loop executes again.
            }
            while (initialValue != Interlocked.CompareExchange(ref totalValue,
                initialValue + addend, initialValue));
            // If no other thread updated the running total, then 
            // totalValue and initialValue are equal when CompareExchange
            // compares them, and computedValue is stored in totalValue.
            // CompareExchange returns the value that was in totalValue
            // before the update, which is equal to initialValue, so the 
            // loop ends.
        }

        static void AddToTotalNoSync(ref float totalValue, float addend)
        {
            totalValue += addend;
        }

        void AddToTotalLock(float addend)
        {
            lock (this)
            {
                _totalValue += addend;
            }
        }


        public double RunTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Non-syncronized access to a float value from multiple threads.
        /// In each thread adds 1 to a total value (initially zeroed) specified number of repetition.
        /// Returns the total value (will be wrong on multi-CPU machines).
        /// RunTime contains the time required for this operation.
        /// </summary>
        public float SumValuesNoSync(int threadCount, int repetitions)
        {
            BlockingThreadPool threadPool = new BlockingThreadPool(threadCount);
            BlockingThreadPool.JobBase[] works = new BlockingThreadPool.JobBase[threadCount];

            for (int t = 0; t < threadCount; ++t)
            {
                works[t] = new BlockingThreadPool.Job<int>
                {
                    Param1 = repetitions,
                    Execute = NoSyncWork
                };
            }

            _totalValue = 0;

            DateTime startTime = DateTime.Now;

            threadPool.ExecuteJobs(works);

            RunTime = (DateTime.Now - startTime).TotalSeconds;

            return _totalValue;
        }

        /// <summary>
        /// Access to a float value from multiple threads synchronized by Interlocked.CompareExchange().
        /// In each thread adds 1 to a total value (initially zeroed) specified number of repetition.
        /// Retunrns the total value.
        /// RunTime contains the time required for this operation.
        /// </summary>
        public float SumValuesInterlocked(int threadCount, int repetitions)
        {
            BlockingThreadPool threadPool = new BlockingThreadPool(threadCount);
            BlockingThreadPool.JobBase[] works = new BlockingThreadPool.JobBase[threadCount];

            for(int t = 0; t < threadCount; ++t)
            {
                works[t] = new BlockingThreadPool.Job<int>
                               {
                                   Param1 = repetitions,
                                   Execute = InterlockedWork
                               };
            }

            _totalValue = 0;

            DateTime startTime = DateTime.Now;

            threadPool.ExecuteJobs(works);

            RunTime = (DateTime.Now - startTime).TotalSeconds;

            return _totalValue;
        }

        /// <summary>
        /// Access to a float value from multiple threads synchronized by lock statement.
        /// In each thread adds 1 to a total value (initially zeroed) specified number of repetition.
        /// Retunrns the total value.
        /// RunTime contains the time required for this operation.
        /// </summary>
        public float SumValuesLock(int threadCount, int repetitions)
        {
            BlockingThreadPool threadPool = new BlockingThreadPool(threadCount);
            BlockingThreadPool.JobBase[] works = new BlockingThreadPool.JobBase[threadCount];

            for (int t = 0; t < threadCount; ++t)
            {
                works[t] = new BlockingThreadPool.Job<int>
                {
                    Param1 = repetitions,
                    Execute = LockWork
                };
            }

            _totalValue = 0;

            DateTime startTime = DateTime.Now;

            threadPool.ExecuteJobs(works);

            RunTime = (DateTime.Now - startTime).TotalSeconds;

            return _totalValue;
        }

        private void InterlockedWork(int repetitions)
        {
            for (int i = 0; i < repetitions; i++)
            {
                AddToTotalInterlocked(ref _totalValue, 1.0f);
            }
        }

        private void NoSyncWork(int repetitions)
        {
            for (int i = 0; i < repetitions; i++)
            {
                AddToTotalNoSync(ref _totalValue, 1.0f);
            }
        }

        private void LockWork(int repetitions)
        {
            for (int i = 0; i < repetitions; i++)
            {
                AddToTotalLock(1.0f);
            }
        }
    }
}
