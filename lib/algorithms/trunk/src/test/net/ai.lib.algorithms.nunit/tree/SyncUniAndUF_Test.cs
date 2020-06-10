/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Runtime.InteropServices;
using ai.lib.utils;
using System.IO;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for SyncUniAndUF. 
    /// </summary>
    [TestFixture]
    public unsafe class SyncUniAndUF_Test
    {
        #region Tests

        /// <summary>
        /// Creates a Uf and a Uni trees with the same structure and data and compares them.
        /// </summary>
        [Test]
        public void Test_Sync()
        {
            int nodesCount = 1 + 4 + 4 * 4 + 4 * 4 * 4;
            int idx;
            TestUfTree ufTree = new TestUfTree(nodesCount);
            idx = 0;
            CreateTestUfTree(ufTree, ref idx, 0, 3, 4);


            TestUniNode uniRoot = new TestUniNode();
            idx = 0;
            CreateTestUniTree(uniRoot, ref idx, 0, 3, 4);

            string userData = "bla";
            SyncUniAndUF<int>.Sync(uniRoot, ufTree, Sync, userData);
        }

        #endregion

        #region Benchmarks


        #endregion

        #region Implementation

        struct TestUfNode 
        {
            public int Id;
        }

        unsafe class TestUfTree: UFTree
        {
            public TestUfTree()
            {
            }

            public TestUfTree(Int64 nodesCount): base(nodesCount, Marshal.SizeOf(typeof(TestUfNode)))
            {
                SetNodesMemory(0);
                AfterRead();
            }

            protected override void AfterRead()
            {
                _nodes = (TestUfNode*)_nodesPtr.Ptr.ToPointer();
            }

            public unsafe TestUfNode * Nodes
            {
                get { return _nodes; }
            }

            public int UserData
            {
                set;
                get;
            }

            TestUfNode* _nodes;
        }

        class TestUniNode
        {
            public static bool TreeGetChild(TestUniNode tree, TestUniNode n, ref int i, out TestUniNode child)
            {
                return (i < n.Children.Length ? child = n.Children[i++] : child = null) != null;
            }
            public int Id;
            public int ChildIndex;
            public TestUniNode[] Children = new TestUniNode[0];
        }



        unsafe void CreateTestUfTree(TestUfTree tree, ref int nodeIdx, int curDepth, int depthLimit, int childCount)
        {
            if (curDepth > depthLimit)
            {
                return;
            }
            tree.SetDepth(nodeIdx, (byte)curDepth);
            tree.Nodes[nodeIdx].Id = nodeIdx++;
            for (int c = 0; c < childCount; ++c)
            {
                CreateTestUfTree(tree, ref nodeIdx, curDepth + 1, depthLimit, childCount);
            }
        }

        void CreateTestUniTree(TestUniNode node, ref int nodeIdx, int curDepth, int depthLimit, int childCount)
        {
            if (curDepth >= depthLimit)
            {
                return;
            }
            node.Children = new TestUniNode[childCount];
            for (int c = 0; c < childCount; ++c)
            {
                node.Children[c] = new TestUniNode { Id = ++nodeIdx, ChildIndex = c};
                CreateTestUniTree(node.Children[c], ref nodeIdx, curDepth + 1, depthLimit, childCount);
            }
        }

        bool Sync(TestUniNode uniTree, TestUniNode uniNode, byte uniDepth, int uniIt, UFTree ufTree, Int64 ufNode, object userData)
        {
            Assert.AreEqual(uniDepth, ufTree.GetDepth(ufNode), String.Format("ufNode: {0}", ufNode));
            TestUfTree tufTree = (TestUfTree)ufTree;
            Assert.AreEqual(uniDepth == 0 ? 0 : uniNode.ChildIndex + 1, uniIt);
            Assert.AreEqual(uniNode.Id, tufTree.Nodes[ufNode].Id);
            Assert.AreEqual("bla", userData);
            return true;
        }

        #endregion
    }
}
