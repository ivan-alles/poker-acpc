/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ai.lib.algorithms;

namespace ai.pkr.holdem.strategy.ca.nunit
{
    /// <summary>
    /// Unit tests for KMeansNode. 
    /// </summary>
    [TestFixture]
    public class KMeansNode_Test
    {
        #region Tests

        [Test]
        public void Test_ReadWrite()
        {
            KMeansNode n = new KMeansNode(2, 0);
            n.Center[0] = 1.111;
            n.Center[1] = 2.222;
            n.ValueMin[0] = 0.1;
            n.ValueMin[1] = 0.2;
            n.ValueBounds[0] = 0.8;
            n.ValueBounds[1] = 0.6;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            n.Write(bw);
            byte []buff = ms.ToArray();
            ms = new MemoryStream(buff);
            BinaryReader br = new BinaryReader(ms);
            KMeansNode n1 = new KMeansNode();
            n1.Read(br, n1.SerializationFormatVersion);
            Assert.AreEqual(n.Center, n1.Center);
            Assert.AreEqual(n.ValueMin, n1.ValueMin);
            Assert.AreEqual(n.ValueBounds, n1.ValueBounds);
            Assert.AreEqual(n.Children, n1.Children);
        }

        [Test]
        public void Test_FindClosestChild()
        {
            KMeansNode root = new KMeansNode(2, 4);
            root.Children.Fill(i => new KMeansNode(2, 0));

            root.Children[0].Center[0] = 0.4922;
            root.Children[0].Center[1] = -0.4344;
            root.Children[1].Center[0] = -0.062;
            root.Children[1].Center[1] = 0.856;
            root.Children[2].Center[0] = 0.8567;
            root.Children[2].Center[1] = 0.5;
            root.Children[3].Center[0] = -0.6567;
            root.Children[3].Center[1] = 0.07333;

            // 2-d data points.
            double[] data = new double[]
            {
              -0.29,   0.17,
               0.56,  -0.36,
               0.90,  -0.18,
               0.92,   0.47,
               0.16,   0.04,
               0.30,  -0.08,
               0.10,   0.88,
               0.74,   0.16,
               0.07,  -0.82,
               0.60,  -0.55,
               0.18,  -0.64,
               0.74,  -0.94,
              -0.97,  -0.22,
               0.91,   0.87,
               0.92,  -0.38,
              -0.71,   0.27,
              -0.51,   0.98,
               0.13,   0.92,
              -0.08,   0.61,
               0.05,   0.89
            };

            int[] expClosestCenters = new int[] { 3, 0, 0, 2, 0, 0, 1, 2, 0, 0, 0, 0, 3, 2, 0, 3, 1, 1, 1, 1 };

            for (int p = 0; p < data.Length / 2; p ++)
            {
                double[] point = new double[] { data[2*p], data[2*p + 1] };
                int closest = root.FindClosestChild(point, false);
                Assert.AreEqual(expClosestCenters[p], closest, p.ToString());
            }
        }




        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
