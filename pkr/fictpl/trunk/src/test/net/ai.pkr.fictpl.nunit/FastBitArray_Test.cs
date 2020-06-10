/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections;

namespace ai.pkr.fictpl.nunit
{
    /// <summary>
    /// Unit tests for FastBitArray. 
    /// </summary>
    [TestFixture]
    public class FastBitArray_Test
    {
        #region Tests

        [Test]
        public void Test_Set_Get()
        {
            uint [] bitCounts = {0, 1, 3, 100, 1000};
            for (int run = 0; run < bitCounts.Length; ++run)
            {
                uint bitCount = bitCounts[run];
                FastBitArray fba = new FastBitArray(bitCount);
                // Fill with a pattern 1111...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, true);
                }
                // Verify
                for (uint i = 0; i < bitCount; ++i)
                {
                    Assert.AreEqual(true, fba.Get(i));
                }
                // Fill with a pattern 00000...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, false);
                }
                // Verify
                for (uint i = 0; i < bitCount; ++i)
                {
                    Assert.AreEqual(false, fba.Get(i));
                }
                // Fill with a pattern 010101...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, i % 2 == 0);
                }
                // Verify
                for (uint i = 0; i < bitCount; ++i)
                {
                    Assert.AreEqual(i % 2 == 0, fba.Get(i));
                }
                // Fill with a pattern 101010...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, i % 2 == 1);
                }
                // Verify
                for (uint i = 0; i < bitCount; ++i)
                {
                    Assert.AreEqual(i % 2 == 1, fba.Get(i));
                }
            }
        }
#if false
        [Test]
        public void Test_ConstIterator()
        {
            uint[] bitCounts = { 1, 2, 3, 100, 1000 };
            for (int run = 0; run < bitCounts.Length; ++run)
            {
                uint bitCount = bitCounts[run];
                FastBitArray fba = new FastBitArray(bitCount);
                // Fill with a pattern 1111...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, true);
                }
                // Verify
                FastBitArray.ConstIterator ci = new FastBitArray.ConstIterator(fba, 0);
                for (uint i = 0; i < bitCount; ++i, ci.Next())
                {
                    Assert.AreEqual(true, ci.Value, i.ToString());
                }
                // Fill with a pattern 00000...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, false);
                }
                // Verify
                ci = new FastBitArray.ConstIterator(fba, 0);
                for (uint i = 0; i < bitCount; ++i, ci.Next())
                {
                    Assert.AreEqual(false, ci.Value);
                }
                // Fill with a pattern 010101...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, i % 2 == 0);
                }
                // Verify
                ci = new FastBitArray.ConstIterator(fba, 0);
                for (uint i = 0; i < bitCount; ++i, ci.Next())
                {
                    Assert.AreEqual(i % 2 == 0, ci.Value);
                }
                // Fill with a pattern 101010...
                for (uint i = 0; i < bitCount; ++i)
                {
                    fba.Set(i, i % 2 == 1);
                }
                // Verify
                ci = new FastBitArray.ConstIterator(fba, 0);
                for (uint i = 0; i < bitCount; ++i, ci.Next())
                {
                    Assert.AreEqual(i % 2 == 1, ci.Value);
                }
            }
        }
#endif
        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Read()
        {
            uint BIT_COUNT = 100000000;
            int REP_COUNT = 10;
            FastBitArray ba = new FastBitArray(BIT_COUNT);
            // Fill with a pattern 0101...
            for (uint i = 0; i < BIT_COUNT; ++i)
            {
                ba.Set(i, (i % 2) == 0);
            }

            bool checksum = false;

            var startTime = DateTime.Now;
            for (int r = 0; r < REP_COUNT; ++r)
            {
                for (uint i = 0; i < BIT_COUNT; ++i)
                {
                    checksum ^= ba.Get(i);
                }
            }
            var runTime = (DateTime.Now - startTime).TotalSeconds;
            double totalBitCount = (double)REP_COUNT * BIT_COUNT;
            Console.WriteLine("Reading bits: count: {0:#,#}, time: {1:0.000} s, {2:#,#} b/s, cs: {0}",
                totalBitCount, runTime, totalBitCount / runTime, checksum);
        }

#if false
        [Test]
        [Category("Benchmark")]
        public void Benchmark_Read_ConstIterator()
        {
            uint BIT_COUNT = 100000000;
            int REP_COUNT = 10;
            FastBitArray ba = new FastBitArray(BIT_COUNT);
            // Fill with a pattern 0101...
            for (uint i = 0; i < BIT_COUNT; ++i)
            {
                ba.Set(i, (i % 2) == 0);
            }

            bool checksum = false;

            var startTime = DateTime.Now;
            for (int r = 0; r < REP_COUNT; ++r)
            {
                FastBitArray.ConstIterator ci = new FastBitArray.ConstIterator(ba, 0);
                for (uint i = 0; i < BIT_COUNT; ++i, ci.Next())
                {
                    checksum ^= ci.Value;
                }
            }
            var runTime = (DateTime.Now - startTime).TotalSeconds;
            double totalBitCount = (double)REP_COUNT * BIT_COUNT;
            Console.WriteLine("Reading bits: count: {0:#,#}, time: {1:0.000} s, {2:#,#} b/s, cs: {0}",
                totalBitCount, runTime, totalBitCount / runTime, checksum);
        }
#endif

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Read_System_Collections_BitArray()
        {
            int BIT_COUNT = 100000000;
            int REP_COUNT = 10;
            BitArray ba = new BitArray(BIT_COUNT);
            // Fill with a pattern 0101...
            for (int i = 0; i < BIT_COUNT; ++i)
            {
                ba.Set(i, (i % 2) == 0);
            }

            bool checksum = false;

            var startTime = DateTime.Now;
            for (int r = 0; r < REP_COUNT; ++r)
            {
                for (int i = 0; i < BIT_COUNT; ++i)
                {
                    checksum ^= ba.Get(i);
                }
            }
            var runTime = (DateTime.Now - startTime).TotalSeconds;
            double totalBitCount = (double)REP_COUNT * BIT_COUNT;
            Console.WriteLine("Reading bits: count: {0:#,#}, time: {1:0.000} s, {2:#,#} b/s, cs: {0}",
                totalBitCount, runTime, totalBitCount / runTime, checksum);
        }
        #endregion

        #region Implementation
        #endregion
    }
}
