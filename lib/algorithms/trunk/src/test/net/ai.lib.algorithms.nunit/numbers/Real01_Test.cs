/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.numbers;

namespace ai.lib.algorithms.numbers.nunit
{
    /// <summary>
    /// Unit tests for Real01. 
    /// </summary>
    [TestFixture]
    public class Real01_Test
    {
        #region Tests

        [Test]
        public void Test_SimpleConvert()
        {
            double[] values = new double[] {1.0, 0.0, 0.5, 0.333, 0.00000001};
            for(int i = 0; i< values.Length; ++i)
            {
                Double value = values[i];
                Real01 r = new Real01(value);
                Double value1 = r.ToDouble();
                Assert.AreEqual(value, value1, Real01.EPSILON);

                // Test conversion operators.
                r = value;
                value1 = (double)r;
                Assert.AreEqual(value, value1, Real01.EPSILON);

                // Test converter of internal representation
                UInt32 data = Real01.FromDouble(value);
                value1 = Real01.ToDouble(data);
                Assert.AreEqual(value, value1, Real01.EPSILON);
            }
        }

        [Test]
        public void Test_RandomConvert()
        {
            int rngSeed = DateTime.Now.Millisecond;
            Console.WriteLine("RNG seed {0}", rngSeed);
            Random rnd = new Random(rngSeed);
            int repetitions = 100000;
            for (int rep = 0; rep < repetitions; ++rep)
            {
                Double value = rnd.NextDouble();
                Real01 r = new Real01(value);
                Double value1 = r.ToDouble();
                Assert.AreEqual(value, value1, Real01.EPSILON);

                // Test conversion operators.
                r = value;
                value1 = (double)r;
                Assert.AreEqual(value, value1, Real01.EPSILON);

                // Test converter of internal representation
                UInt32 data = Real01.FromDouble(value);
                value1 = Real01.ToDouble(data);
                Assert.AreEqual(value, value1, Real01.EPSILON);
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
