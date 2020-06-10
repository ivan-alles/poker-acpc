/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.random;
using NUnit.Framework;
using ai.lib.algorithms.numbers;

namespace ai.lib.algorithms.random.nunit
{
    [TestFixture]
    public class DiscreteProbabilityRng_Test
    {
        #region Tests
        [Test]
        public void Test_CheckWeights_System_Random()
        {
            CheckWeights(new double[] { 1 }, new System.Random());
            CheckWeights(new double[] { 0, 1 }, new System.Random());
            CheckWeights(new double[] { 1, 0 }, new System.Random());
            CheckWeights(new double[] { 0, 0, 1 }, new System.Random());
            CheckWeights(
                new double[] { 0, 0, 1, 2, 4, 8, 0, 0, 0, 0, 16, 8, 4, 0, 0, 0, 0, 2, 0, 1, 0, 0, 0, 0, },
                new System.Random());
        }

        [Test]
        public void Test_CheckWeights_MersenneTwister()
        {
            CheckWeights(new double[] { 0, 1 }, new MersenneTwister());
            CheckWeights(new double[] { 1, 0 }, new MersenneTwister());
            CheckWeights(new double[] { 0, 0, 1 }, new MersenneTwister());
            CheckWeights(
                new double[] { 0, 0, 1, 2, 4, 8, 0, 0, 0, 0, 16, 8, 4, 0, 0, 0, 0, 2, 0, 1, 0, 0, 0, 0, },
                new MersenneTwister());
        }

        /// <summary>
        /// Make sure the same result is generated for the same seed.
        /// </summary>
        [Test]
        public void Test_SameSeed()
        {
            double[] weights = (new double[256]).Fill(i => i);

            int seed = 123;
            DiscreteProbabilityRng rng1;
            DiscreteProbabilityRng rng2;

            // Case 1
            rng1 = new DiscreteProbabilityRng(seed);
            rng2 = new DiscreteProbabilityRng(seed);
            rng1.SetWeights(weights);
            rng2.SetWeights(weights);

            for (int i = 0; i < 100; ++i)
            {
                Assert.AreEqual(rng1.Next(), rng2.Next());
            }

            // Case 2
            rng1 = new DiscreteProbabilityRng(new Random(seed));
            rng2 = new DiscreteProbabilityRng(new Random(seed));
            rng1.SetWeights(weights);
            rng2.SetWeights(weights);

            for (int i = 0; i < 100; ++i)
            {
                Assert.AreEqual(rng1.Next(), rng2.Next());
            }
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Test_Benchmark_System_Random()
        {
            Benchmark(new System.Random());
        }

        [Test]
        [Category("Benchmark")]
        public void Test_Benchmark_MersenneTwister()
        {
            Benchmark(new MersenneTwister());
        }

        #endregion

        #region Implementation

        public void CheckWeights(double[] weights, Random underlyingRng)
        {
            int[] counts = new int[weights.Length];
            DiscreteProbabilityRng rng = new DiscreteProbabilityRng(underlyingRng);
            rng.SetWeights(weights);
            for (int i = 0; i < 200000; ++i)
            {
                int r = rng.Next();
                counts[r]++;
            }
            double[] resultWeights = new double[weights.Length];

            int sum = 0;
            double sumWeights = 0;
            for (int i = 0; i < counts.Length; ++i)
            {
                Console.Write("{0} ", counts[i]);
                sum += counts[i];
                sumWeights += weights[i];
            }
            Console.WriteLine();
            for (int i = 0; i < counts.Length; ++i)
            {
                resultWeights[i] = (double)counts[i] / sum * sumWeights;
                Console.Write("{0:0.000} ", resultWeights[i]);
                Assert.IsTrue(FloatingPoint.AreEqualRel(weights[i], resultWeights[i], 0.05));
                if (weights[i] == 0)
                {
                    Assert.AreEqual(0, counts[i], "0-weigths must not occur");
                }
            }
            Console.WriteLine();
        }

        public void Benchmark(Random underlyingRng)
        {
            double[] weights = new double[1000];
            for (int i = 0; i < weights.Length; ++i)
            {
                weights[i] = i;
            }

            DiscreteProbabilityRng rng = new DiscreteProbabilityRng(underlyingRng);
            rng.SetWeights(weights);

            int REPETITIONS = 50000000;
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < REPETITIONS; ++i)
            {
                rng.Next();
            }
            TimeSpan time = DateTime.Now - startTime;
            Console.Write("{0} Repetitions done in {1:0.0} s, {2:0,0} numbers/s",
                REPETITIONS, time.TotalSeconds, REPETITIONS / time.TotalSeconds);
        }

        #endregion
    }
}
