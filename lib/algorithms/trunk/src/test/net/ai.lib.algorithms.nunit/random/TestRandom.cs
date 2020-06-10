/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ai.lib.algorithms.random.nunit
{
    /// <summary>
    /// Unit tests for an RNG implementing System.Random().
    /// </summary>
    class TestRandom
    {
        public TestRandom(Random rng, string outDir)
        {
            _outDir = outDir;
            _rng = rng;
        }

        /// <summary>
        /// Creates a file of random UInt32 elements by calling NextDouble() method.
        /// This method is chosen as we know that this is the underlying generator
        /// for other methods of System.Random.
        /// </summary>
        /// <param name="repCount"></param>
        public void CreateFileForDiehard(int approxFileSize)
        {
            string fileName = Path.Combine(_outDir, _rng.ToString() + ".dat");
            int repCount = approxFileSize/4 + 1;
            using (BinaryWriter wr = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write)))
            {
                for (int r = 0; r < repCount; ++r)
                {
                    double d = _rng.NextDouble();
                    UInt32 u = (UInt32)(d*UInt32.MaxValue);
                    wr.Write(u);
                }
            }
        }

        public void BenchmarkNextDouble(int repCount)
        {
            DateTime start = DateTime.Now;
            // Calculate and print checksum to prevent optimizing away
            double checksum = 0;
            for (int r = 0; r < repCount; ++r)
            {
                checksum = _rng.NextDouble();
            }
            double time = (DateTime.Now - start).TotalSeconds;
            Console.Out.WriteLine("{0}: time: {1:0.0000} s, {2:#,#} num/s, checksum {3}",
                _rng, time, repCount / time, checksum);
        }

        private string _outDir;
        private Random _rng;

    }
}
