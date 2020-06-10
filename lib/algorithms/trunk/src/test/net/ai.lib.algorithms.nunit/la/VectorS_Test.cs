/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.lib.algorithms.la.nunit
{
    /// <summary>
    /// Unit tests for VectorS. 
    /// </summary>
    [TestFixture]
    public class VectorS_Test
    {
        #region Tests


        [Test]
        public void Test_Volume()
        {
            Assert.AreEqual(2, VectorS.Volume(new double[] { 2 }));
            Assert.AreEqual(6, VectorS.Volume(new double[] { 2,3 }));
            Assert.AreEqual(30, VectorS.Volume(new double[] { 2,5,3 }));
        }

        [Test]
        public void Test_AreEqual()
        {
            Assert.IsTrue(VectorS.AreEqual(new double[] { 1, 3 }, new double[] { 1, 3 }));
            Assert.IsFalse(VectorS.AreEqual(new double[] { 1, 3 }, new double[] { 1, 3, 3 }));
            Assert.IsFalse(VectorS.AreEqual(new double[] { 1, 3 }, new double[] { 1, 2 }));
        }

        [Test]
        public void Test_SquaredDistance()
        {
            Assert.AreEqual(2, VectorS.SquaredDistance(new double[] {1, 3}, new double[] {2, 4}));
        }

        [Test]
        public void Test_Distance()
        {
            Assert.AreEqual(5, VectorS.Distance(new double[] { -1, 2 }, new double[] { 2, 6 }));
        }

        [Test]
        public void Test_Add()
        {
            Assert.AreEqual(new double[]{2,3}, VectorS.Add(new double[] { -1, 2 }, new double[] { 3, 1 }));
        }

        [Test]
        public void Test_Sub()
        {
            Assert.AreEqual(new double[] { 2, 3 }, VectorS.Sub(new double[] { -1, 2 }, new double[] { -3, -1 }));
        }

        [Test]
        public void Test_UpdateMin()
        {
            double[] min = new double[2].Fill(double.MaxValue);
            VectorS.UpdateMin(new double[] {1, 3}, min);
            Assert.AreEqual(new double[]{1,3}, min);
            VectorS.UpdateMin(new double[] { 5, 0.4 }, min);
            Assert.AreEqual(new double[] { 1, 0.4 }, min);
        }

        [Test]
        public void Test_UpdateMax()
        {
            double[] max = new double[2].Fill(double.MinValue);
            VectorS.UpdateMax(new double[] { 1, 3 }, max);
            Assert.AreEqual(new double[] { 1, 3 }, max);
            VectorS.UpdateMax(new double[] { 5, 0.4 }, max);
            Assert.AreEqual(new double[] { 5, 3 }, max);
        }

        [Test]
        public void Test_Normalize()
        {
            double[] min = new double[2] {1, 2};
            double[] max = new double[2] {5, 7 };
            double[] v = new double[2] {3,4};
            VectorS.Normalize(v, min, max);
            Assert.AreEqual(new double[] {0.5, 0.4}, v);
        }

        [Test]
        public void Test_NormalizeByDiff()
        {
            double[] min = new double[2] { 1, 2 };
            double[] diff = new double[2] { 4, 5 };
            double[] v = new double[2] { 3, 4 };
            VectorS.NormalizeByDiff(v, min, diff);
            Assert.AreEqual(new double[] { 0.5, 0.4 }, v);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
