/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ai.lib.algorithms.tree;
using ai.lib.utils;
using System.Reflection;

namespace ai.pkr.holdem.strategy.ca.nunit
{
    /// <summary>
    /// Unit tests for ClusterTree. 
    /// </summary>
    [TestFixture]
    public class ClusterTree_Test
    {
        #region Tests

        [Test]
        public void Test_ReadWrite()
        {
            ClusterTree rt1 = new ClusterTree();
            RangeNode root = new RangeNode(5);
            rt1.Root = root;
            for (int i = 0; i < 5; ++i)
            {
                root.Children[i] = new RangeNode(0) { UpperLimit = 0.1f * i };
            }
            rt1.Version.UserDescription = "Bla bla";

            string fileName = Path.Combine(_outDir, "ClusterTree.dat");
            rt1.Write(fileName);
            ClusterTree rt2 = ClusterTree.Read(fileName);

            Assert.AreEqual(rt1.Version, rt2.Version);
            Assert.IsTrue(CompareTrees<int, int>.Compare(rt1, rt1.Root, rt2, rt2.Root,
                (t1, n1, t2, n2) =>
                {
                    return ((RangeNode)n1).UpperLimit == ((RangeNode)n2).UpperLimit;
                }));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "ClusterTree_Test");

        #endregion
    }
}
