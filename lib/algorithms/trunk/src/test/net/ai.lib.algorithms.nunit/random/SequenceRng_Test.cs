/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;
using System.IO;
using System.Reflection;
using ai.lib.utils;
using ai.lib.algorithms.random;

namespace ai.lib.algorithms.random.nunit
{
    /// <summary>
    /// Unit tests for SequenceRng. 
    /// </summary>
    [TestFixture]
    public class SequenceRng_Test
    {
        #region Tests

        [Test]
        public void Test_Shuffle()
        { 
            // Make sure that shuffling from some start does not overwrite the beginning of the sequence
            int [] sequence = new int[10].Fill(i => i);
            SequenceRng sr = new SequenceRng();
            for(int start = 0; start <= sequence.Length; start++)
            {
                sr.SetSequence(sequence);
                sr.Shuffle(start, sequence.Length - start);
                for(int i = 0; i < start; ++i)
                {
                    Assert.AreEqual(i, sr.Sequence[i]);
                }
            }
        }

        /// <summary>
        /// Make sure the same sequence is generated for the same seed.
        /// </summary>
        [Test]
        public void Test_SameSeed()
        {
            int[] sequence = (new int[256]).Fill(i => i);

            int seed = 123;
            SequenceRng rng1;
            SequenceRng rng2;

            // Case 1
            rng1 = new SequenceRng(seed);
            rng2 = new SequenceRng(seed);
            rng1.SetSequence(sequence);
            rng2.SetSequence(sequence);
            rng1.Shuffle();
            rng2.Shuffle();
            Assert.AreEqual(rng1.Sequence, rng2.Sequence);

            // Case 2
            rng1 = new SequenceRng(seed, sequence);
            rng2 = new SequenceRng(seed, sequence);
            rng1.Shuffle();
            rng2.Shuffle();
            Assert.AreEqual(rng1.Sequence, rng2.Sequence);

            // Case 3
            rng1 = new SequenceRng(new Random(seed));
            rng2 = new SequenceRng(new Random(seed));
            rng1.SetSequence(sequence);
            rng2.SetSequence(sequence);
            rng1.Shuffle();
            rng2.Shuffle();
            Assert.AreEqual(rng1.Sequence, rng2.Sequence);

            // Case 4
            rng1 = new SequenceRng(new Random(seed), sequence);
            rng2 = new SequenceRng(new Random(seed), sequence);
            rng1.Shuffle();
            rng2.Shuffle();
            Assert.AreEqual(rng1.Sequence, rng2.Sequence);

        }

        /// <summary>
        /// Generate files for diehard test.
        /// </summary>
        [Test]
        [Explicit]
        public void Test_Diehard()
        {
            CreateFileForDiehard(new Random(), 12 * 1000000);
            CreateFileForDiehard(new MersenneTwister(), 12 * 1000000);
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

        void CreateFileForDiehard(Random rng, int approxFileSize)
        {
            int [] sequence = (new int[256]).Fill(i => i);

            string fileName = Path.Combine(_outDir, rng.ToString() + ".dat");

            SequenceRng seqRng = new SequenceRng(rng, sequence);

            int repCount = approxFileSize/sequence.Length + 1;

            using (BinaryWriter wr = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write)))
            {
                for (int r = 0; r < repCount; ++r)
                {
                    seqRng.Shuffle();
                    for (int i = 0; i < seqRng.Sequence.Length; ++i)
                    {
                        byte b = (byte) seqRng.Sequence[i];
                        wr.Write(b);
                    }
                }
            }
        }

        public void Benchmark(Random underlyingRng)
        {
            int[] array = new int[100].Fill(i => i);

            SequenceRng rng = new SequenceRng(underlyingRng, array);

            int repCount = 500000;
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < repCount; ++i)
            {
                rng.Shuffle();
            }
            TimeSpan time = DateTime.Now - startTime;
            long totalRepCount = array.Length * repCount;
            Console.Write("{0:#,#} Repetitions done in {1:0.0} s, {2:#,#} shuffles/s",
                totalRepCount, time.TotalSeconds, totalRepCount / time.TotalSeconds);
        }

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "random/SequenceRng_Test");


        #endregion
    }
}
