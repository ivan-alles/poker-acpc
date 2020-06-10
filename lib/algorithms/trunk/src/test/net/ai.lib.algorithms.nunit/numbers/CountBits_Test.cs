/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.numbers;
using ai.lib.algorithms.lut;

namespace ai.lib.algorithms.numbers.nunit
{
    [TestFixture]
    public unsafe class CountBits_Test
    {
        [Test]
        public void Test_Values()
        {
            Assert.AreEqual(0, CountBits.Count((byte) 0x00));
            Assert.AreEqual(0, CountBits.Count((sbyte)0x00));
            Assert.AreEqual(0, CountBits.Count((UInt16)0x0000));
            Assert.AreEqual(0, CountBits.Count((Int16)0x0000));
            Assert.AreEqual(0, CountBits.Count((UInt32)0x00000000));
            Assert.AreEqual(0, CountBits.Count((Int32)0x00000000));
            Assert.AreEqual(0, CountBits.Count((UInt64)0x0000000000000000));
            Assert.AreEqual(0, CountBits.Count((Int64)0x0000000000000000));

            Assert.AreEqual(4, CountBits.Count((byte)0xF0));
            Assert.AreEqual(4, CountBits.Count((sbyte)0x0F));
            Assert.AreEqual(4, CountBits.Count((UInt16)0x0F00));
            Assert.AreEqual(4, CountBits.Count((Int16)0x000F));
            Assert.AreEqual(4, CountBits.Count((UInt32)0x00F00000));
            Assert.AreEqual(4, CountBits.Count((Int32)0x00000F00));
            Assert.AreEqual(4, CountBits.Count((UInt64)0x000000000F000000));
            Assert.AreEqual(4, CountBits.Count((Int64)0x0F00000000000000));
        }

        [Test]
        public void Test_1Bit()
        {
            for (int b = 0; b < 8; ++b)
            {
                Assert.AreEqual(1, CountBits.Count((byte)(1 << b)));
                Assert.AreEqual(1, CountBits.Count((sbyte)(1 << b)));
            }
            for (int b = 0; b < 16; ++b)
            {
                Assert.AreEqual(1, CountBits.Count((UInt16)(1 << b)));
                Assert.AreEqual(1, CountBits.Count((Int16)(1 << b)));
            }
            for (int b = 0; b < 32; ++b)
            {
                Assert.AreEqual(1, CountBits.Count((UInt32)(1 << b)));
                Assert.AreEqual(1, CountBits.Count((Int32)(1 << b)));
            }
            for (int b = 0; b < 64; ++b)
            {
                Assert.AreEqual(1, CountBits.Count((UInt64)(1ul << b)));
                Assert.AreEqual(1, CountBits.Count((Int64)(1ul << b)));
            }
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark()
        {
            DateTime startTime;
            int checksum = 0;
            int repetitions;
            UInt16 value16 = 0;

            repetitions = 0x2FFFFFFF;

            checksum = 0;
            startTime = DateTime.Now;
            value16 = 0x333;
            for (UInt32 value = 0; value < repetitions; value++)
            {
                checksum += CountBits.Count(value16);
            }
            PrintResult("UInt16", repetitions, (DateTime.Now - startTime).TotalSeconds, checksum);

            checksum = 0;
            repetitions = 0x2FFFFFFF;
            startTime = DateTime.Now;
            for (UInt32 value = 0; value < repetitions; value++)
            {
                checksum += CountBits.Count(value);
            }
            PrintResult("UInt32", repetitions, (DateTime.Now - startTime).TotalSeconds, checksum);

            checksum = 0;
            repetitions = 0x2FFFFFFF;
            startTime = DateTime.Now;
            for (UInt32 value = 0; value < repetitions; value++)
            {
                checksum += CountBits.Count(0xF0F0F0F0F0F0F0F0);
            }
            PrintResult("UInt64", repetitions, (DateTime.Now - startTime).TotalSeconds, checksum);
        }

        private void PrintResult(string type, int count, double duration, int checksum)
        {
            Console.WriteLine("{0} {1:###.###} s, {2:###,###,###,###} values/s, {3} repetitions, checksum: {4}.", 
                type, duration, count/duration, count, checksum);
        }


    }
}
