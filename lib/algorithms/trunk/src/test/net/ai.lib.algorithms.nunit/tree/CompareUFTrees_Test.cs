/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.tree;
using System.Runtime.InteropServices;
using ai.lib.utils;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for CompareUFTrees. 
    /// </summary>
    [TestFixture]
    public unsafe class CompareUFTrees_Test
    {
        #region Tests

        [Test]
        public void Test_Equal()
        {
            int expectedNodesCount = 1 + 4 + 4 * 4 + 4 * 4 * 4;
            TestTree tree1 = new TestTree(expectedNodesCount);
            TestTree tree2 = new TestTree(expectedNodesCount);
            int idx = 0;
            CreateTestTree(tree1, idx, ref idx, 0, 3, 4);
            idx = 0;
            CreateTestTree(tree2, idx, ref idx, 0, 3, 4);

            CompareUFTrees<TestTree, TestTree> comp = new CompareUFTrees<TestTree, TestTree>();
            bool result = comp.Compare(tree1, tree2, (t1, t2, n) => t1.Nodes[n].Value == t2.Nodes[n].Value);
            Assert.IsTrue(result);
            Assert.AreEqual(CompareUFTrees.ResultKind.Equal, comp.Result);
        }

        [Test]
        public void Test_StructureDiffers_DifferentNodesCount()
        {
            int expectedNodesCount = 1 + 4 + 4*4 + 4*4*4;
            TestTree tree1 = new TestTree(expectedNodesCount);
            int idx = 0;
            CreateTestTree(tree1, idx, ref idx, 0, 3, 4);
            expectedNodesCount = 1 + 4 + 4*4;
            TestTree tree2 = new TestTree(expectedNodesCount);
            idx = 0;
            CreateTestTree(tree2, idx, ref idx, 0, 2, 4);

            CompareUFTrees<TestTree, TestTree> comp = new CompareUFTrees<TestTree, TestTree>();
            bool result = comp.Compare(tree1, tree2, (t1, t2, n) => t1.Nodes[n].Value == t2.Nodes[n].Value);
            Assert.IsFalse(result);
            Assert.AreEqual(CompareUFTrees.ResultKind.StructureDiffers, comp.Result);
            Assert.AreEqual(-1, comp.DiffersAt);
        }

        [Test]
        public void Test_StructureDiffers_SameNodesCount()
        {
            int expectedNodesCount = 1 + 4 + 4 * 4;
            TestTree tree1 = new TestTree(expectedNodesCount);
            int idx = 0;
            CreateTestTree(tree1, idx, ref idx, 0, 2, 4);
            TestTree tree2 = new TestTree(expectedNodesCount);
            idx = 0;
            CreateTestTree(tree2, idx, ref idx, 0, 2, 4);

            // Now move node 19 (last node in PP to be a child) of the root,
            // and set node 20 as child of node 19.
            Assert.AreEqual(2, tree2.GetDepth(19));
            Assert.AreEqual(2, tree2.GetDepth(20));
            tree2.SetDepth(19, (byte)1);
            tree2.SetDepth(20, (byte)2);

            CompareUFTrees<TestTree, TestTree> comp = new CompareUFTrees<TestTree, TestTree>();
            bool result = comp.Compare(tree1, tree2, (t1, t2, n) => t1.Nodes[n].Value == t2.Nodes[n].Value);
            Assert.IsFalse(result);
            Assert.AreEqual(CompareUFTrees.ResultKind.StructureDiffers, comp.Result);
            Assert.AreEqual(19, comp.DiffersAt);
        }

        [Test]
        public void Test_ValueDiffers()
        {
            int expectedNodesCount = 1 + 4 + 4 * 4 + 4 * 4 * 4;
            TestTree tree1 = new TestTree(expectedNodesCount);
            TestTree tree2 = new TestTree(expectedNodesCount);
            int idx = 0;
            CreateTestTree(tree1, idx, ref idx, 0, 3, 4);
            idx = 0;
            CreateTestTree(tree2, idx, ref idx, 0, 3, 4);

            tree2.Nodes[4].Value = -1000;

            CompareUFTrees<TestTree, TestTree> comp = new CompareUFTrees<TestTree, TestTree>();
            bool result = comp.Compare(tree1, tree2, (t1, t2, n) => t1.Nodes[n].Value == t2.Nodes[n].Value);
            Assert.IsFalse(result);
            Assert.AreEqual(CompareUFTrees.ResultKind.ValueDiffers, comp.Result);
            Assert.AreEqual(4, comp.DiffersAt);
        }

        #endregion

        #region Benchmarks

        #endregion

        #region Implementation

        struct TestNode
        {
            public int Value;
        }

        unsafe class TestTree : UFTree
        {
            public TestTree(Int64 nodesCount)
                : base(nodesCount, Marshal.SizeOf(typeof(TestNode)))
            {
                UnmanagedMemory.SetMemory(_nodesPtr.Ptr, _nodesByteSize, 0);
                _nodes = (TestNode*)_nodesPtr.Ptr.ToPointer();
            }

            public TestNode* Nodes
            {
                get { return _nodes; }
            }

            private TestNode* _nodes;
        }

        unsafe void CreateTestTree(TestTree tree, int nodeImmutableIdx, ref int nodeIdx, int curDepth, int depthLimit, int childCount)
        {
            if (curDepth > depthLimit)
            {
                return;
            }
            Assert.AreEqual(nodeImmutableIdx, nodeIdx);
            tree.SetDepth(nodeIdx, (byte)curDepth);
            tree.Nodes[nodeIdx].Value = nodeIdx;
            nodeIdx++;
            for (int c = 0; c < childCount; ++c)
            {
                CreateTestTree(tree, nodeIdx, ref nodeIdx, curDepth + 1, depthLimit, childCount);
            }
        }
        #endregion
    }
}
