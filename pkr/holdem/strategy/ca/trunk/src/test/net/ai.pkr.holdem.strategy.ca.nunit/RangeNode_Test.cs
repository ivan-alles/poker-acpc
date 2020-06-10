/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.holdem.strategy.ca.nunit
{
    /// <summary>
    /// Unit tests for RangeNode. 
    /// </summary>
    [TestFixture]
    public class RangeNode_Test
    {
        #region Tests

        [Test]
        public void Test_FindChildByValue()
        {
            RangeNode n = new RangeNode(1);
            n.Children[0] = new RangeNode(0) { UpperLimit = 1 };
            Assert.AreEqual(0, n.FindChildByValue(0f));

            n.Children[0].UpperLimit = float.PositiveInfinity;
            Assert.AreEqual(0, n.FindChildByValue(0f));
            Assert.AreEqual(0, n.FindChildByValue(float.MaxValue));

            n = new RangeNode(4);
            for (int i = 0; i < 4; ++i)
            {
                n.Children[i] = new RangeNode(0) { UpperLimit = 0.2f * i };
            }
            n.Children[3].UpperLimit = float.PositiveInfinity;
            Assert.AreEqual(0, n.FindChildByValue(float.NegativeInfinity));
            Assert.AreEqual(0, n.FindChildByValue(-1f));
            Assert.AreEqual(1, n.FindChildByValue(0f));
            Assert.AreEqual(1, n.FindChildByValue(0.1f));
            Assert.AreEqual(2, n.FindChildByValue(0.2f));
            Assert.AreEqual(2, n.FindChildByValue(0.3f));
            Assert.AreEqual(3, n.FindChildByValue(0.4f));
            Assert.AreEqual(3, n.FindChildByValue(0.5f));
            Assert.AreEqual(3, n.FindChildByValue(float.MaxValue));


            n = new RangeNode(5);
            for (int i = 0; i < 5; ++i)
            {
                n.Children[i] = new RangeNode(0) { UpperLimit = 0.2f * i };
            }
            n.Children[4].UpperLimit = float.PositiveInfinity;
            Assert.AreEqual(0, n.FindChildByValue(float.NegativeInfinity));
            Assert.AreEqual(0, n.FindChildByValue(-1f));
            Assert.AreEqual(1, n.FindChildByValue(0f));
            Assert.AreEqual(1, n.FindChildByValue(0.1f));
            Assert.AreEqual(2, n.FindChildByValue(0.2f));
            Assert.AreEqual(2, n.FindChildByValue(0.3f));
            Assert.AreEqual(3, n.FindChildByValue(0.4f));
            Assert.AreEqual(3, n.FindChildByValue(0.5f));
            Assert.AreEqual(4, n.FindChildByValue(0.6f));
            Assert.AreEqual(4, n.FindChildByValue(0.7f));
            Assert.AreEqual(4, n.FindChildByValue(float.MaxValue));

        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_FindChildByValue()
        {
            RangeNode n = new RangeNode(10);
            for (int i = 0; i < 10; ++i)
            {
                n.Children[i] = new RangeNode(0) { UpperLimit = i };
            }
            n.Children[9].UpperLimit = float.PositiveInfinity;

            int repCount = 1000000;
            int min = -1;
            int max = 11;
            int cs = 0;
            DateTime startTime = DateTime.Now;
            for (int r = 0; r < repCount; ++r)
            {
                for (int v = min; v <= max; ++v)
                {
                    cs += n.FindChildByValue((float)v);
                }
            }
            double time = (DateTime.Now - startTime).TotalSeconds;
            long totalRepCount = (long)(max - min + 1) * repCount;
            Console.WriteLine("Repetitions: {0:#,#}, time: {1:0.000} s, {2:#,#} r/s",
                totalRepCount, time, totalRepCount / time);
        }

        #endregion

        #region Implementation
        #endregion
    }
}
