/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.algorithms.nunit
{
    /// <summary>
    /// Unit tests for ContainerExtensions. 
    /// </summary>
    [TestFixture]
    public class ContainerExtensions_Test
    {
        #region Tests

        [Test]
        public void Test_Rotate_List()
        {
            List<int> seq;

            seq = new List<int>(new int[] { 0, 1 });
            seq.Rotate(0);
            Assert.AreEqual(new int[] { 0, 1 }, seq);

            seq = new List<int>(new int[] { 0, 1 });
            seq.Rotate(1);
            Assert.AreEqual(new int[] { 1, 0 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2 });
            seq.Rotate(1);
            Assert.AreEqual(new int[] { 2, 0, 1 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2 });
            seq.Rotate(2);
            Assert.AreEqual(new int[] { 1, 2, 0 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3 });
            seq.Rotate(1);
            Assert.AreEqual(new int[] { 3, 0, 1, 2 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3 });
            seq.Rotate(2);
            Assert.AreEqual(new int[] { 2, 3, 0, 1 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3 });
            seq.Rotate(3);
            Assert.AreEqual(new int[] { 1, 2, 3, 0 }, seq);


            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.Rotate(1);
            Assert.AreEqual(new int[] { 6, 0, 1, 2, 3, 4, 5 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.Rotate(2);
            Assert.AreEqual(new int[] { 5, 6, 0, 1, 2, 3, 4 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.Rotate(3);
            Assert.AreEqual(new int[] { 4, 5, 6, 0, 1, 2, 3 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.Rotate(4);
            Assert.AreEqual(new int[] { 3, 4, 5, 6, 0, 1, 2 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.Rotate(5);
            Assert.AreEqual(new int[] { 2, 3, 4, 5, 6, 0, 1 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.Rotate(6);
            Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 0 }, seq);
        }

        class TestFill
        {
            public int Value;
        }

        [Test]
        public void Test_Fill()
        {
            int[] a1 = new int[4].Fill(3);
            Assert.AreEqual(4, a1.Length);
            for (int i = 0; i < a1.Length; ++i)
            {
                Assert.AreEqual(3, a1[i]);
            }

            a1 = new int[3].Fill(i => i);
            Assert.AreEqual(3, a1.Length);
            for (int i = 0; i < a1.Length; ++i)
            {
                Assert.AreEqual(i, a1[i]);
            }

            TestFill[] a2 = new TestFill[5].Fill(i => new TestFill{Value = i});
            Assert.AreEqual(5, a2.Length);
            for (int i = 0; i < a1.Length; ++i)
            {
                Assert.AreEqual(i, a2[i].Value);
            }
        }

        [Test]
        public void Test_RotateMinCopy_List()
        {
            List<int> seq;

            seq = new List<int>(new int[]{0,1});
            seq.RotateMinCopy(0);
            Assert.AreEqual(new int[]{0, 1}, seq);

            seq = new List<int>(new int[] { 0, 1 });
            seq.RotateMinCopy(1);
            Assert.AreEqual(new int[] { 1, 0 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2 });
            seq.RotateMinCopy(1);
            Assert.AreEqual(new int[] { 2, 0, 1 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2 });
            seq.RotateMinCopy(2);
            Assert.AreEqual(new int[] { 1, 2, 0 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3 });
            seq.RotateMinCopy(1);
            Assert.AreEqual(new int[] { 3, 0, 1, 2 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3 });
            seq.RotateMinCopy(2);
            Assert.AreEqual(new int[] { 2, 3, 0, 1 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3 });
            seq.RotateMinCopy(3);
            Assert.AreEqual(new int[] { 1, 2, 3, 0 }, seq);


            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.RotateMinCopy(1);
            Assert.AreEqual(new int[] { 6, 0, 1, 2, 3, 4, 5 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.RotateMinCopy(2);
            Assert.AreEqual(new int[] { 5, 6, 0, 1, 2, 3, 4 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.RotateMinCopy(3);
            Assert.AreEqual(new int[] { 4, 5, 6, 0, 1, 2, 3}, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.RotateMinCopy(4);
            Assert.AreEqual(new int[] { 3, 4, 5, 6, 0, 1, 2 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.RotateMinCopy(5);
            Assert.AreEqual(new int[] { 2, 3, 4, 5, 6, 0, 1 }, seq);

            seq = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            seq.RotateMinCopy(6);
            Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 0 }, seq);
        }

        #endregion

        #region Benchmarks

        public class Cls
        {
            public int i;
        }

        public class Str
        {
            public int i;
            public int i1;
            public int i2;
            public int i3;
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Rotate_List()
        {
            int repetitions = 2000000;

            List<int> seq;
            seq = new List<int>(new int[100]);

            seq.Rotate(2); // Force JIT.

            DateTime startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq.Rotate(2);
            }
            PrintResult("seq<int>[100].Rotate(2)", repetitions, (DateTime.Now - startTime).TotalSeconds);

            startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq.Rotate(3);
            }
            PrintResult("seq<int>[100].Rotate(3)", repetitions, (DateTime.Now - startTime).TotalSeconds);

            List<Cls> seq1 = new List<Cls>(100);
            for (int i = 0; i < 100; ++i)
            {
                seq1.Add(new Cls());
            }

            startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq1.Rotate(3);
            }
            PrintResult("seq<Cls>[100].Rotate(3)", repetitions, (DateTime.Now - startTime).TotalSeconds);

            List<Str> seq2 = new List<Str>(100);
            for (int i = 0; i < 100; ++i)
            {
                seq2.Add(new Str());
            }

            startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq2.Rotate(3);
            }
            PrintResult("seq<Str>[100].Rotate(3)", repetitions, (DateTime.Now - startTime).TotalSeconds);

        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_RotateMinCopy_List()
        {
            int repetitions = 2000000;

            List<int> seq;
            seq = new List<int>(new int[100]);

            seq.RotateMinCopy(2); // Force JIT.

            DateTime startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq.RotateMinCopy(2);
            }
            PrintResult("seq<int>[100].RotateMinCopy(2)", repetitions, (DateTime.Now - startTime).TotalSeconds);

            startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq.RotateMinCopy(3);
            }
            PrintResult("seq<int>[100].RotateMinCopy(3)", repetitions, (DateTime.Now - startTime).TotalSeconds);

            List<Cls> seq1 = new List<Cls>(100);
            for(int i = 0; i < 100; ++i)
            {
                seq1.Add(new Cls());
            }

            startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq1.RotateMinCopy(3);
            }
            PrintResult("seq<Cls>[100].RotateMinCopy(3)", repetitions, (DateTime.Now - startTime).TotalSeconds);

            List<Str> seq2 = new List<Str>(100);
            for (int i = 0; i < 100; ++i)
            {
                seq2.Add(new Str());
            }

            startTime = DateTime.Now;
            for (int r = 0; r < repetitions; r++)
            {
                seq2.RotateMinCopy(3);
            }
            PrintResult("seq<Str>[100].RotateMinCopy(3)", repetitions, (DateTime.Now - startTime).TotalSeconds);

        }

        #endregion

        #region Implementation
        private void PrintResult(string prefix, int count, double duration)
        {
            Console.WriteLine("{0} {1:###.###} s, {2:###,###,###,###} r/s, {3} repetitions.",
                prefix, duration, count / duration, count);
        }

        #endregion
    }
}
