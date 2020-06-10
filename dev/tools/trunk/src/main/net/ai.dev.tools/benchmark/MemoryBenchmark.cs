/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ai.dev.tools.benchmark
{
    /// <summary>
    /// Determines efficience of various ways of memory allocation.
    /// For example, answers the question: how can more memory be allocated, in one large chunk
    /// or in many small chuncks.
    /// </summary>
    public unsafe class MemoryBenchmark
    {
        /// <summary>
        /// If true, prints messages to console.
        /// </summary>
        public bool IsVerbose
        {
            set;
            get;
        }

        /// <summary>
        /// If true, prints messages about each iteration to console.
        /// If one function iterates over another function, which, in turn, iterates too,
        /// will influence usually only the latter function.
        /// </summary>
        public bool IsIterationVerbose
        {
            set;
            get;
        }

        /// <summary>
        /// Allocates arrays of given type with given element count until memory is exhausted and 
        /// returns the number of bytes totally allocated. 
        /// </summary>
        public UInt64 AllocateArrayExhaustive<T>(int elCount) where T : struct
        {
            int sizeOf = Marshal.SizeOf(typeof(T));

            Console.WriteLine("AllocateArrayExhaustive started: el size: {0}, el count: {1}", sizeOf, elCount);
            int arrayCount = 0;
            UInt64 totalSize = 0;
            List<T[]> _arrays = new List<T[]>();
            bool isMemoryExhausted = false;
            try
            {
                for (arrayCount = 0; arrayCount <= int.MaxValue; ++arrayCount)
                {
                    _arrays.Add(new T[elCount]);
                    totalSize = (UInt64)sizeOf * (UInt64)elCount * (UInt64)arrayCount;
                    if (IsIterationVerbose)
                    {
                        Console.WriteLine(
                            "AllocateArrayExhaustive current: el size: {0}, el count: {1}, arrays allocated: {2:0,0}, bytes allocated: {3:0,0}",
                            sizeOf, elCount, arrayCount, totalSize);
                    }
                }
            }
            catch(OutOfMemoryException )
            {
                isMemoryExhausted = true;
            }
            _arrays.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (isMemoryExhausted && IsVerbose)
            {
                Console.WriteLine("AllocateArrayExhaustive: memory exhaused.");
            }
            if(IsVerbose)
            {
                Console.WriteLine(
                    "AllocateArrayExhaustive current: el size: {0}, el count: {1}, arrays allocated: {2:0,0}, bytes allocated: {3:0,0}",
                    sizeOf, elCount, arrayCount, totalSize);
            }
            return totalSize;
        }

        /// <summary>
        /// Calls AllocateArrayExhaustive() for given range of element counts, each time multiplying the count by 2,
        /// and finds the maximally allocated memory size.
        /// </summary>
        public UInt64 AllocateArrayExhaustiveBest<T>(int elCountBegin, int elCountEnd, out int bestElCount) where T : struct
        {
            Console.WriteLine("AllocateArrayExhaustiveBest started: el count begin: {0}, end: {1}", elCountBegin, elCountEnd);
            UInt64 best = UInt64.MinValue;
            bestElCount = -1;
            for(int elCount = elCountBegin; ; elCount *= 2)
            {
                elCount = Math.Min(elCountEnd, elCount);
                UInt64 cur = AllocateArrayExhaustive<T>(elCount);
                if (cur > best)
                {
                    best = cur;
                    bestElCount = elCount;
                }
                if(elCount == elCountEnd)
                {
                    break;
                }
            }
            Console.WriteLine("AllocateArrayExhaustiveBest finished: el count begin: {0}, end: {1}, best result: {2:0,0} bytes for el count {3}",
                elCountBegin, elCountEnd, best, bestElCount);
            return best;
        }

    }
}
