/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.stdpoker.nunit
{
    /// <summary>
    /// Unit tests for HandValueToOrdinal.
    /// </summary>
    [TestFixture]
    public unsafe class HandValueToOrdinal_Test
    {
        #region Tests

        [Test]
        public void Test_GetOrdinal7()
        {
            HashSet<UInt32> distinct = new HashSet<uint>();
            for (int c1 = 52 - 1; c1 >= 6; --c1)
            {
                UInt32 v1 = LutEvaluator7.pLut[c1];
                for (int c2 = c1 - 1; c2 >= 5; --c2)
                {
                    UInt32 v2 = LutEvaluator7.pLut[v1 + c2];
                    for (int c3 = c2 - 1; c3 >= 4; --c3)
                    {
                        UInt32 v3 = LutEvaluator7.pLut[v2 + c3];
                        for (int c4 = c3 - 1; c4 >= 3; --c4)
                        {
                            UInt32 v4 = LutEvaluator7.pLut[v3 + c4];
                            for (int c5 = c4 - 1; c5 >= 2; --c5)
                            {
                                UInt32 v5 = LutEvaluator7.pLut[v4 + c5] + (uint) c5 - 1;
                                for (int c6 = c5 - 1; c6 >= 1; --c6, --v5)
                                {
                                    UInt32 v6 = LutEvaluator7.pLut[v5] + (uint) c6 - 1;
                                    for (int c7 = c6 - 1; c7 >= 0; --c7, --v6)
                                    {
                                        UInt32 rank = LutEvaluator7.pLut[v6];
                                        distinct.Add(rank);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            UInt32[] lut = new uint[distinct.Count];
            int i = 0;
            foreach (UInt32 value in distinct)
            {
                lut[i++] = value;
            }
            Array.Sort(lut);
            Assert.AreEqual(4824, lut.Length);

            for (int c1 = 52 - 1; c1 >= 6; --c1)
            {
                UInt32 v1 = LutEvaluator7.pLut[c1];
                for (int c2 = c1 - 1; c2 >= 5; --c2)
                {
                    UInt32 v2 = LutEvaluator7.pLut[v1 + c2];
                    for (int c3 = c2 - 1; c3 >= 4; --c3)
                    {
                        UInt32 v3 = LutEvaluator7.pLut[v2 + c3];
                        for (int c4 = c3 - 1; c4 >= 3; --c4)
                        {
                            UInt32 v4 = LutEvaluator7.pLut[v3 + c4];
                            for (int c5 = c4 - 1; c5 >= 2; --c5)
                            {
                                UInt32 v5 = LutEvaluator7.pLut[v4 + c5] + (uint) c5 - 1;
                                for (int c6 = c5 - 1; c6 >= 1; --c6, --v5)
                                {
                                    UInt32 v6 = LutEvaluator7.pLut[v5] + (uint) c6 - 1;
                                    for (int c7 = c6 - 1; c7 >= 0; --c7, --v6)
                                    {
                                        UInt32 val = LutEvaluator7.pLut[v6];
                                        int ordinal = HandValueToOrdinal.GetOrdinal7(val);
                                        int expOrdinal = Array.BinarySearch(lut, val);
                                        Assert.AreEqual(expOrdinal, ordinal, val.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
