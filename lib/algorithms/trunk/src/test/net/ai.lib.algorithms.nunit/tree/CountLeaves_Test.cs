/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.tree;
using System.Collections;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for CountLeaves. 
    /// </summary>
    [TestFixture]
    public class CountLeaves_Test
    {
        #region Tests

        /// <summary>
        /// Creates a test tree and walks it using indexed child access.
        /// </summary>
        [Test]
        public void Test_CountLeaves()
        {
            TestNode root = new TestNode();
            int depth = 3;
            int width = 4;
            CreateTestTree(ref root, depth, width);
            int leavesCount = CountLeaves<int>.Count(root, root);
            Assert.AreEqual((int) Math.Pow(width, depth), leavesCount);

            depth = 8;
            width = 3;
            CreateTestTree(ref root, depth, width);
            leavesCount = CountLeaves<int>.Count(root, root);
            Assert.AreEqual((int)Math.Pow(width, depth), leavesCount);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class TestNode
        {
            public static bool TreeGetChild(TestNode tree, TestNode n, ref int i, out TestNode child)
            {
                return (i < n.Children.Length ? child = n.Children[i++] : child = null) != null;
            }

            public int Value;
            public TestNode[] Children;

            public override string ToString()
            {
                return String.Format("v:{0}", Value);
            }
        }

        void CreateTestTree(ref TestNode node, int depth, int childCount)
        {
            if (depth == 0)
            {
                node.Children = new TestNode[0];
                return;
            }
            node.Children = new TestNode[childCount];
            for (int c = 0; c < childCount; ++c)
            {
                node.Children[c] = new TestNode();
                node.Children[c].Value = c;
                CreateTestTree(ref node.Children[c], depth - 1, childCount);
            }
        }

        #endregion
    }
}
