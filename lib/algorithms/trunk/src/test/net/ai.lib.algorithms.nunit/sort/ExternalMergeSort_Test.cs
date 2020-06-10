/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using System.Reflection;
using ai.lib.algorithms.sort;
using ai.lib.algorithms.random;
using System.IO;

namespace ai.lib.algorithms.sort.nunit
{
    /// <summary>
    /// Unit tests for ExternalMergeSort. 
    /// </summary>
    [TestFixture]
    public class ExternalMergeSort_Test
    {
        #region Tests

        [Test]
        public void Test_Sort()
        {
            TestSort(1000000, new int[]{100000,200000,700000}, 150000, 1, 1000000, true);
        }

        [Test]
        public void Test_Sort_Random()
        {
            int rngSeed = DateTime.Now.Millisecond;
            Console.WriteLine("RNG seed: {0}", rngSeed);
            
            Random rng = new Random(rngSeed);

            int repetitions = 10;

            for (int r = 0; r < repetitions; ++r)
            {

                int arraySize = rng.Next(0, 1000000);
                int partsCount = rng.Next(1, 100);
                int[] partSizes = new int[partsCount];
                int totalPartSize = 0;
                for (int p = 0; p < partsCount - 1; ++p)
                {
                    partSizes[p] = rng.Next(0, arraySize - totalPartSize);
                    totalPartSize += partSizes[p];
                }
                partSizes[partsCount - 1] = arraySize - totalPartSize;

                int inMemSortSize = rng.Next(1000, 2000000);
                int rngSeed1 = rng.Next();
                int shuffleSize = rng.Next(0, arraySize);

                TestSort(arraySize, partSizes, inMemSortSize, rngSeed1, shuffleSize, false);
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class TestElement: ExternalMergeSort<int>.IElement
        {

            #region IElement Members

            public void Write(int element, System.IO.BinaryWriter w)
            {
                w.Write(element);
            }

            public int Read(System.IO.BinaryReader r)
            {
                return r.ReadInt32();
            }

            #endregion

            #region IComparer<int> Members

            public int Compare(int x, int y)
            {
                if(x < y)
                {
                    return -1;
                }
                if (x > y)
                {
                    return 1;
                }
                return 0;
            }

            #endregion
        }

        void TestSort(int arraySize, int [] partSizes, int inMemSortSize, int rngSeed, int shuffleSize, bool isVerbose)
        {
            string inputDir = Path.Combine(_outDir, "input");
            DirectoryExt.Delete(inputDir);
            Directory.CreateDirectory(inputDir);

            string tempDir = Path.Combine(_outDir, "temp");

            int[] array = new int[arraySize];
            for(int i = 0; i<array.Length;++i)
            {
                array[i] = i;
            }

            SequenceRng rng = new SequenceRng(rngSeed);
            rng.SetSequenceNoCopy(array);
            rng.Shuffle(shuffleSize);

            TestElement element = new TestElement();

            int begin = 0;
            for(int p = 0; p < partSizes.Length; ++p)
            {
                string fileName = Path.Combine(inputDir, string.Format("{0}.dat", p));
                using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
                {
                    for(int i = begin; i < begin + partSizes[p]; ++i)
                    {
                        element.Write(array[i], writer);
                    }
                    begin += partSizes[p];
                }
            }

            Assert.AreEqual(arraySize, begin, "Incorrect part sizes.");

            string resultFile = Path.Combine(_outDir, "result.dat");

            ExternalMergeSort<int> sorter = new ExternalMergeSort<int>{IsVerbose = isVerbose};
            sorter.Sort(inputDir, tempDir, inMemSortSize, element, resultFile);

            int[] result = ReadToArray(resultFile, element);

            for(int i = 0; i < arraySize; ++i)
            {
                Assert.AreEqual(i, result[i]);
            }
        }

        int[] ReadToArray(string fileName, ExternalMergeSort<int>.IElement element)
        {
            FileInfo f = new FileInfo(fileName);
            int[] arr = new int[f.Length / 4];
            int count = 0;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader r = new BinaryReader(fs))
                {
                    for (; fs.Position < f.Length; )
                    {
                        arr[count++] = element.Read(r);
                    }
                }
            }
            return arr;
        }

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "sort/ExternalMergeSort_Test");

        #endregion
    }
}
