/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy.algorithms;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.algorithms;
using System.Diagnostics;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for NormSuit. 
    /// </summary>
    [TestFixture]
    public class NormSuit_Test
    {
        #region Tests

        [Test]
        public void Test_Convert()
        {
            // Do generic testing with 1 suit in card set.
            TestConvertOneSuite(0xFFFF);
            TestConvertOneSuite(0x8001);

            // Do some more complex testing manually.
            NormSuit sn = new NormSuit();
            Assert.AreEqual(FromSuits(0, 0, 0x8001, 0x8001), sn.Convert(FromSuits(0x8001, 0, 0, 0x8001)));
            sn = new NormSuit();
            Assert.AreEqual(FromSuits(0, 0, 0x7001, 0xFFFF), sn.Convert(FromSuits(0, 0xFFFF, 0, 0x7001)));
            sn.Reset();
            Assert.AreEqual(FromSuits(0, 0, 0x8001, 0xFFFF), sn.Convert(FromSuits(0, 0xFFFF, 0, 0x8001)));
            sn.Reset();
            Assert.AreEqual(FromSuits(0, 0x8001, 0xF00F, 0xFFFF), sn.Convert(FromSuits(0xF00F, 0x8001, 0, 0xFFFF)));
            sn.Reset();
            Assert.AreEqual(FromSuits(0x8001, 0xDEAD, 0xF00F, 0xFFFF), sn.Convert(FromSuits(0xFFFF, 0xF00F, 0x8001, 0xDEAD)));

            // Make sure equal suites keep order for 2 suits
            for (int s1 = 0; s1 < 4; ++s1)
            {
                for (int s2 = s1 + 1; s2 < 4; ++s2)
                {
                    sn.Reset();
                    uint[] s = new uint[4];
                    s[s1] = 1;
                    s[s2] = 1;
                    // Convert once with equal suit ranks.
                    sn.Convert(FromSuits(s));
                    // Change ranks, make sure they are ordered as suite indexes.
                    s[s1] = 1;
                    s[s2] = 2;
                    Assert.AreEqual(FromSuits(0, 0, 2, 1), sn.Convert(FromSuits(s)));
                }
            }

            // Make sure equal suites keep order for 3 suits
            for (int s1 = 0; s1 < 4; ++s1)
            {
                for (int s2 = s1 + 1; s2 < 4; ++s2)
                {
                    for (int s3 = s2 + 1; s3 < 4; ++s3)
                    {
                        sn.Reset();
                        uint[] s = new uint[4];
                        s[s1] = 1;
                        s[s2] = 1;
                        s[s3] = 1;
                        // Convert once with equal suit ranks.
                        sn.Convert(FromSuits(s));
                        // Change ranks, make sure they are ordered as suite indexes.
                        s[s1] = 1;
                        s[s2] = 2;
                        s[s3] = 3;
                        Assert.AreEqual(FromSuits(0, 3, 2, 1), sn.Convert(FromSuits(s)));
                    }
                }
            }

            // Make sure equal suites keep order for 4 suits - only one combination is possible
            sn.Reset();
            sn.Convert(FromSuits(1, 1, 1, 1));
            Assert.AreEqual(FromSuits(4, 3, 2, 1), sn.Convert(FromSuits(4, 3, 2, 1)));


            // Test all permutations of some suits combinations.
            TestPermutations(0x1, 0x2, 0x3, 0x4);
            TestPermutations(0x8001, 0xFFFF, 0xF0F0, 0x0F0F);
            TestPermutations(0x0, 0x0, 0x0, 0x0);
            TestPermutations(0x0, 0x0, 0x0, 0x1);
            TestPermutations(0x0, 0x0, 0x1, 0x1);
            TestPermutations(0x0, 0x0, 0x2, 0x1);
            TestPermutations(0x0, 0x1, 0x1, 0x1);
            TestPermutations(0x0, 0x2, 0x1, 0x1);
            TestPermutations(0x0, 0x3, 0x2, 0x1);
            TestPermutations(0x1, 0x1, 0x1, 0x1);
            TestPermutations(0x2, 0x1, 0x1, 0x1);
            TestPermutations(0x2, 0x2, 0x1, 0x1);
            TestPermutations(0x3, 0x2, 0x1, 0x1);
        }

        [Test]
        public void Test_CountSuits()
        {
            // Total 16 combinations
            Assert.AreEqual(0, NormSuit.CountSuits(FromSuits(0, 0, 0, 0)));
            Assert.AreEqual(1, NormSuit.CountSuits(FromSuits(0, 0, 0, 1)));
            Assert.AreEqual(1, NormSuit.CountSuits(FromSuits(0, 0, 1, 0)));
            Assert.AreEqual(1, NormSuit.CountSuits(FromSuits(0, 1, 0, 0)));
            Assert.AreEqual(1, NormSuit.CountSuits(FromSuits(1, 0, 0, 0)));
            Assert.AreEqual(2, NormSuit.CountSuits(FromSuits(0, 0, 1, 1)));
            Assert.AreEqual(2, NormSuit.CountSuits(FromSuits(0, 1, 0, 1)));
            Assert.AreEqual(2, NormSuit.CountSuits(FromSuits(1, 0, 0, 1)));
            Assert.AreEqual(2, NormSuit.CountSuits(FromSuits(0, 1, 1, 0)));
            Assert.AreEqual(2, NormSuit.CountSuits(FromSuits(1, 0, 1, 0)));
            Assert.AreEqual(2, NormSuit.CountSuits(FromSuits(1, 1, 0, 0)));
            Assert.AreEqual(3, NormSuit.CountSuits(FromSuits(0, 1, 1, 1)));
            Assert.AreEqual(3, NormSuit.CountSuits(FromSuits(1, 0, 1, 1)));
            Assert.AreEqual(3, NormSuit.CountSuits(FromSuits(1, 1, 0, 1)));
            Assert.AreEqual(3, NormSuit.CountSuits(FromSuits(1, 1, 1, 0)));
            Assert.AreEqual(4, NormSuit.CountSuits(FromSuits(1, 1, 1, 1)));
        }

        #endregion

        #region Benchmarks
        [Test]
        [Category("Benchmark")]
        public void Benchmark_Convert()
        {
            int repetitions = 40000000;

            CardSet cs1 = FromSuits(0, 0x1, 0, 0);
            CardSet cs2 = FromSuits(0x1, 0, 0, 0);
            CardSet cs3 = FromSuits(0, 0, 0x1, 0);
            CardSet cs4 = FromSuits(0, 0, 0, 0x1);

            NormSuit sn = new NormSuit();
            
            DateTime startTime = DateTime.Now;

            for(int i = 0; i < repetitions; ++i)
            {
                CardSet result;
                result = sn.Convert(cs1);
                result = sn.Convert(cs2);
                result = sn.Convert(cs3);
                result = sn.Convert(cs4);
            }

            double time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine("Single card: conversions {0:###,###,###}, time {1} s, {2:###,###,###} conv/s",
                repetitions * 4, time, repetitions * 4 / time);

            cs1 = FromSuits(0, 0xFFFF, 0, 0);
            cs2 = FromSuits(0xFFFF, 0, 0, 0);
            cs3 = FromSuits(0, 0, 0xFFFF, 0);
            cs4 = FromSuits(0, 0, 0, 0xFFFF);

            sn = new NormSuit();

            startTime = DateTime.Now;

            for (int i = 0; i < repetitions; ++i)
            {
                CardSet result;
                result = sn.Convert(cs1);
                result = sn.Convert(cs2);
                result = sn.Convert(cs3);
                result = sn.Convert(cs4);
            }

            time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine("Full suite: conversions {0:###,###,###}, time {1} s, {2:###,###,###} conv/s",
                repetitions * 4, time, repetitions * 4 / time);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_CopyFrom()
        {
            int repetitions = 40000000;

            NormSuit sn = new NormSuit();
            NormSuit sn1 = new NormSuit();

            DateTime startTime = DateTime.Now;

            for (int i = 0; i < repetitions; ++i)
            {
                sn1.CopyFrom(sn);
            }

            double time = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine("Repetitions {0:###.###.###}, time {1} s, {2:###,###,###,###} copy/s",
                repetitions, time, repetitions / time);
        }
        #endregion

        #region Implementation

        CardSet FromSuits(uint s3, uint s2, uint s1, uint s0)
        {
            CardSet cs = new CardSet();
            cs.bits = (ulong)s0 + ((ulong)s1 << 16) + ((ulong)s2 << 32) + ((ulong)s3 << 48);
            return cs;
        }

        CardSet FromSuits(uint[] s)
        {
            CardSet cs = new CardSet();
            cs.bits = (ulong)s[0] + ((ulong)s[1] << 16) + ((ulong)s[2] << 32) + ((ulong)s[3] << 48);
            return cs;
        }
        /// <summary>
        /// 1. Selects a starting suit
        /// 2. Converts a pattern of one suite 4 times, setting the pattern first to the starting suite,
        ///    then to next suit, etc.
        /// </summary>
        /// <param name="pattern"></param>
        private void TestConvertOneSuite(UInt32 pattern)
        {
            NormSuit sn = new NormSuit();

            for (int s1 = 0; s1 < 4; ++s1)
            {
                // Console.WriteLine("Starting suite {0}", s1);
                CardSet orig = new CardSet();
                orig.bits = (ulong)pattern << 16 * (s1 % 4);
                Assert.AreEqual(FromSuits(0, 0, 0, pattern), sn.Convert(orig));
                orig.bits = (ulong)pattern << 16 * ((s1 + 1) % 4);
                Assert.AreEqual(FromSuits(0, 0, pattern, 0), sn.Convert(orig));

                // Test copy constructor.
                NormSuit sn2 = new NormSuit(sn); 

                orig.bits = (ulong)pattern << 16 * ((s1 + 2) % 4);
                Assert.AreEqual(FromSuits(0, pattern, 0, 0), sn2.Convert(orig));
                orig.bits = (ulong)pattern << 16 * ((s1 + 3) % 4);
                Assert.AreEqual(FromSuits(pattern, 0, 0, 0), sn2.Convert(orig));

                // Test both reset and recreate.
                if (s1 % 2 == 0)
                    sn.Reset();
                else
                    sn = new NormSuit();
            }
        }

        /// <summary>
        /// Generates all possible hands containing given suites,
        /// normalizes and verifies them.
        /// </summary>
        private void TestPermutations(uint s3, uint s2, uint s1, uint s0)
        {
            uint[] arr = new uint[] {s0, s1, s2, s3};
            Array.Sort(arr);
            CardSet expected = FromSuits(arr[0], arr[1], arr[2], arr[3]);
            List<List<int>> perms = EnumAlgos.GetPermut(4);
            for (int i = 0; i < perms.Count; ++i)
            {
                // Console.WriteLine("Permutation: {0}", i);
                NormSuit sn = new NormSuit();
                CardSet orig = FromSuits(arr[perms[i][0]], arr[perms[i][1]], arr[perms[i][2]], arr[perms[i][3]]);
                CardSet converted = sn.Convert(orig);
                Assert.AreEqual(expected, converted);
                // Make sure result is consistent.
                converted = sn.Convert(orig);
                Assert.AreEqual(expected, converted);
                // Make sure self is converted to self with a new normalizers
                // (because it is already sorted).
                NormSuit sn2 = new NormSuit();
                converted = sn2.Convert(converted);
                Assert.AreEqual(expected, converted);
            }
        }

        [Test]
        [Explicit]
        public void SortSuitesCodeGenerator()
        {
            List<List<int>> perms = EnumAlgos.GetPermut(4);
            for(int i = 0; i < perms.Count; ++i)
            {
                Console.Write("{");
                for (int j = 0; j < 4; ++j)
                {
                    if(j != 0) Console.Write(",");
                    Console.Write(" {0}", perms[i][j]);
                }
                Console.WriteLine("}}, // {0,2}", i);
            }
            Generate(perms, perms, "");
        }

        private void Generate(List<List<int>> allPerms, List<List<int>> perms, string indent)
        {
            if(perms.Count == 1)
            {
                for(int p = 0; p < allPerms.Count; ++p)
                {
                    if(allPerms[p].Equals(perms[0]))
                    {
                        Console.WriteLine("{0}return {1};", indent, p);
                        return;
                    }
                }
                Debug.Assert(false); // Not found ? 
            }
            List<List<int>> right = null;
            List<List<int>> wrong = null;
            int i1 = -1, i2 = -1;
            for (i1 = 0; i1 < 4; i1++)
            {
                for (i2 = 0; i2 < 4; i2++)
                {
                    right = new List<List<int>>();
                    wrong = new List<List<int>>();
                    for(int p= 0; p < perms.Count; ++p)
                    {
                        int pos1 = -1;
                        int pos2 = -1;
                        for (int pos = 0; pos < perms[p].Count; ++pos)
                        {
                            if (perms[p][pos] == i1) pos1 = pos;
                            if (perms[p][pos] == i2) pos2 = pos;
                        }
                        Debug.Assert(pos1 != -1 && pos2 != -1);
                        if (pos1 < pos2)
                            right.Add(perms[p]);
                        else
                            wrong.Add(perms[p]);
                    }
                    Debug.Assert(right.Count + wrong.Count == perms.Count);
                    if(Math.Abs(right.Count - wrong.Count) <= 1)
                    {
                        goto found;
                    }
                }
            }
        found:
            Console.WriteLine("{0}if(s{1} >= s{2}) {{", indent, i1, i2);
            Generate(allPerms, right, indent + "  ");
            Console.WriteLine("{0}}}", indent);
            Console.WriteLine("{0}else {{", indent);
            Generate(allPerms, wrong, indent + "  ");
            Console.WriteLine("{0}}}", indent);
        }
        #endregion
    }
}
