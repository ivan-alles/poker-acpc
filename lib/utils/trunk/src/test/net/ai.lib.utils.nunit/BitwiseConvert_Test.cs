/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// Unit tests for BitwiseConvert. 
    /// </summary>
    [TestFixture]
    public class BitwiseConvert_Test
    {
        #region Tests

        [Test]
        public void Test_Double_UInt32Arr()
        {
            int rngSeed = DateTime.Now.Millisecond;
            Console.WriteLine("RNG seed {0}", rngSeed);
            Random rnd = new Random(rngSeed);
            int repetitions = 100000;
            UInt32[] data = new UInt32[2];
            for (int rep = 0; rep < repetitions; ++rep)
            {
                Double value = rnd.NextDouble()*Double.MaxValue + Double.MinValue/2;
                BitwiseConvert.ToUInt32Arr(value, data);
                Double value1 = BitwiseConvert.ToDouble(data);
                Assert.AreEqual(value, value1);
            }
        }

        [Test]
        public void Test_UInt64_UInt32Arr()
        {
            int rngSeed = DateTime.Now.Millisecond;
            Console.WriteLine("RNG seed {0}", rngSeed);
            Random rnd = new Random(rngSeed);
            int repetitions = 100000;
            UInt32[] data = new UInt32[2];
            for (int rep = 0; rep < repetitions; ++rep)
            {
                // Generate in a range larger than max value and wrap.
                UInt64 value = unchecked((UInt64)(2.0 * rnd.NextDouble() * (Double)UInt64.MaxValue));
                BitwiseConvert.ToUInt32Arr(value, data);
                UInt64 value1 = BitwiseConvert.ToUInt64(data);
                Assert.AreEqual(value, value1);
            }
        }
        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_BitConverter()
        {
            int repetitions = 20000000;

            DateTime startTime = DateTime.Now;
            uint dummyCount = 0; // To avoid optimization
            for (int i = 0; i < repetitions; ++i)
            {
                byte[] data = BitConverter.GetBytes(1.0);
                dummyCount += data[0];
            }

            double time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine(
                "BitConverter.GetBytes(double): repetitions {0:###,###,###}, dummy-count {1}, time {2} s, {3:###,###,###} conv/s",
                repetitions, dummyCount, time, repetitions / time);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Converter()
        {
            int repetitions = 20000000;
            DateTime startTime;
            double time;
            uint dummyCount; // To avoid optimization
            double dummyCountDouble;


            UInt32[] data = new UInt32[2];

            startTime = DateTime.Now;
            dummyCount = 0; // To avoid optimization
            for (int i = 0; i < repetitions; ++i)
            {
                BitwiseConvert.ToUInt32Arr(1.0, data);
                dummyCount += data[0];
            }

            time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine(
                "BitwiseConvert.ToUInt32Arr(double, UInt32[]): repetitions {0:###,###,###}, dummy-count {1}, time {2} s, {3:###,###,###} conv/s",
                repetitions, dummyCount, time, repetitions / time);

            startTime = DateTime.Now;
            dummyCountDouble = 0; 
            for (int i = 0; i < repetitions; ++i)
            {
                dummyCountDouble += BitwiseConvert.ToDouble(data);
            }

            time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine(
                "BitwiseConvert.ToDoble(UInt32[]): repetitions {0:###,###,###}, dummy-count {1}, time {2} s, {3:###,###,###} conv/s",
                repetitions, dummyCountDouble, time, repetitions / time);
        }
        #endregion

        #region Implementation
        #endregion
    }
}
