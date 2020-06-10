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
using System.Reflection;
using System.IO;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for UFToUniTree. 
    /// </summary>
    [TestFixture]
    public unsafe class UFToUniAdapter_Test
    {
        #region Tests

        [Test]
        public void Test_Walk_FromMemory()
        {
            int expectedNodesCount = 1 + 4 + 4 * 4;// +4 * 4 * 4;
            TestTree tree = new TestTree(expectedNodesCount);
            int idx = 0;
            CreateTestTree(tree, ref idx, 0, 2, 4);
            UFToUniAdapter adapter = new UFToUniAdapter(tree);
            VerifyWalk(tree, adapter);
        }

        [Test]
        public  void Test_Walk_FromFile()
        {
            int expectedNodesCount = 1 + 4 + 4 * 4;// +4 * 4 * 4;
            TestTree tree = new TestTree(expectedNodesCount);
            int idx = 0;
            CreateTestTree(tree, ref idx, 0, 2, 4);
            UFToUniAdapter adapter = new UFToUniAdapter(tree, Path.Combine(_outDir, "index.dat"), false);
            VerifyWalk(tree, adapter);
        }

        private void VerifyWalk(TestTree tree, UFToUniAdapter adapter)
        {
            List<int> expectedPre = new List<int>();
            List<int> expectedPost = new List<int>();

            WalkUFTreePP<TestTree, WalkUFTreePPContext> wft =
                new WalkUFTreePP<TestTree, WalkUFTreePPContext>();
            wft.OnNodeBegin = (t, s, d) => expectedPre.Add(tree.Nodes[s[d].NodeIdx].Value);
            wft.OnNodeEnd = (t, s, d) => expectedPost.Add(tree.Nodes[s[d].NodeIdx].Value);

            wft.Walk(tree);

            List<int> actualPre = new List<int>();
            List<int> actualPost = new List<int>();

            WalkTreePP<UFToUniAdapter, int, int, AdapterContext> wt = new WalkTreePP<UFToUniAdapter, int, int, AdapterContext>();
            wt.OnNodeBegin = (t, n, s, d) =>
                                 {
                                     actualPre.Add(tree.Nodes[n].Value);
                                     return true;
                                 };
            wt.OnNodeEnd = (t, n, s, d) => actualPost.Add(tree.Nodes[n].Value);

            wt.Walk(adapter, 0);

            Assert.AreEqual(expectedPre, actualPre);
            Assert.AreEqual(expectedPost, actualPost);

            int bi, cnt;
            adapter.GetChildrenBeginIdxAndCount(0, out bi, out cnt);
            Assert.AreEqual(4, cnt);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        private string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "tree/UFToUniAdapter_Test");

        class AdapterContext: WalkTreePPContext<int, int>
        {
                
        }

        struct TestNode
        {
            public int Value;

            public override string ToString()
            {
                return String.Format("v:{0}", Value);
            }
        }

        unsafe class TestTree : UFTree
        {
            public TestTree(Int64 nodesCount)
                : base(nodesCount, Marshal.SizeOf(typeof(TestNode)))
            {
                UnmanagedMemory.SetMemory(_nodesPtr.Ptr, _nodesByteSize, 0);
                _nodes = (TestNode*)_nodesPtr.Ptr.ToPointer();
            }

            public unsafe TestNode* Nodes
            {
                get { return _nodes; }
            }

            private TestNode* _nodes;
        }

        unsafe void CreateTestTree(TestTree tree, ref int nodeIdx, int curDepth, int depthLimit, int childCount)
        {
            if (curDepth > depthLimit)
            {
                return;
            }
            tree.SetDepth(nodeIdx, (byte) curDepth);
            tree.Nodes[nodeIdx].Value = nodeIdx;
            nodeIdx++;
            for (int c = 0; c < childCount; ++c)
            {
                CreateTestTree(tree, ref nodeIdx, curDepth + 1, depthLimit, childCount);
            }
        }
        #endregion
    }
}
