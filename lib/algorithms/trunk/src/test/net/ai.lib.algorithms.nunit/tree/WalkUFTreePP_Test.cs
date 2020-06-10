/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Runtime.InteropServices;
using ai.lib.utils;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for WalkUFTreePP. 
    /// </summary>
    [TestFixture]
    public unsafe class WalkUFTreePP_Test
    {
        #region Tests

        /// <summary>
        /// Creates a test tree, walks it and verifies the results.
        /// </summary>
        [Test]
        public unsafe void Test_Walk()
        {
            DoWalkTest(0, 1 + 4 + 4 * 4 + 4 * 4 * 4, 3, 4, true);
            DoWalkTest(1, 1 + 4 + 4 * 4, 3, 4, true);
            DoWalkTest(2, 1 + 4, 3, 4, true);
            DoWalkTest(3, 1, 3, 4, true);
            DoWalkTest(4, 1, 3, 4, false);
            DoWalkTest(22, 1 + 4 + 4 * 4, 3, 4, false);
            DoWalkTest(64, 1 + 4 + 4 * 4, 3, 4, false);
            DoWalkTest(80, 1 + 4, 3, 4, false);
            DoWalkTest(83, 1, 3, 4, false);
            DoWalkTest(84, 1, 3, 4, false);
        }
        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public unsafe void Benchmark_Walk()
        {
            int depthLimit = 25;
            int expectedNodesCount = (1 << (depthLimit + 1)) - 1;
            TestTree tree = new TestTree(expectedNodesCount);
            int idx = 0;
            CreateTestTree(tree, idx, ref idx, 0, depthLimit, 2);

            WalkUFTreePP<TestTree, Context> walker =
                new WalkUFTreePP<TestTree, Context>();

            int nodeCount = 0;

            walker.OnNodeBegin = (t, s, d) => { nodeCount++; };
            
            DateTime startTime = DateTime.Now;

            walker.Walk(tree);

            double time = (DateTime.Now - startTime).TotalSeconds;

            Assert.AreEqual(expectedNodesCount, nodeCount);

            Console.WriteLine("Node count {0:###,###,###}, {1} s, {2:###,###,###} n/s",
                nodeCount, time, nodeCount / time);
        }

        #endregion

        #region Implementation

        class Context: WalkUFTreePPContext
        {
        }

        struct TestNode 
        {
            public byte BeginCallCount;
            public byte EndCallCount;
            public int PreValue;
            public int PostValue;

            public override string ToString()
            {
                return String.Format("v:{0}", PreValue);
            }
        }

        unsafe class  TestTree : UFTree
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

        int _postValue = 0;
        unsafe void CreateTestTree(TestTree tree, int nodeImmutableIdx, ref int nodeIdx, int curDepth, int depthLimit, int childCount)
        {
            if (curDepth > depthLimit)
            {
                return;
            }
            Assert.AreEqual(nodeImmutableIdx, nodeIdx);
            tree.SetDepth(nodeIdx, (byte) curDepth);
            tree.Nodes[nodeIdx].PreValue = nodeIdx;
            nodeIdx++;
            for (int c = 0; c < childCount; ++c)
            {
                CreateTestTree(tree, nodeIdx, ref nodeIdx, curDepth + 1, depthLimit, childCount);
            }
            tree.Nodes[nodeImmutableIdx].PostValue = _postValue++;
        }


        void DoWalkTest(int startNode, int expectedNodesCount, int depth, int childCount, bool checkPostValue)
        {
            int power = 1;
            int nodesCount = 0;
            for (int d = 0; d <= depth; ++d)
            {
                nodesCount += power;
                power *= childCount;
            }
            TestTree tree = new TestTree(nodesCount);
            int idx = 0;
            _postValue = 0;
            CreateTestTree(tree, idx, ref idx, 0, 3, 4);

            WalkUFTreePP<TestTree, WalkUFTreePPContext> wt = new WalkUFTreePP<TestTree, WalkUFTreePPContext>();

            int treeBeginCount = 0;
            int treeEndCount = 0;
            int nodeBeginCount = 0;
            int nodeEndCount = 0;

            wt.OnNodeBegin = (t, s, d) =>
            {
                Assert.AreEqual(1, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                Assert.AreEqual(0, t.Nodes[s[d].NodeIdx].BeginCallCount);
                Assert.AreEqual(0, t.Nodes[s[d].NodeIdx].EndCallCount);
                Assert.AreEqual(nodeBeginCount + startNode, t.Nodes[s[d].NodeIdx].PreValue);
                Assert.AreEqual(0, s[d].ChildrenCount);
                if (d > 0)
                {
                    Assert.IsTrue(s[d - 1].ChildrenCount >= 1);
                    Assert.IsTrue(s[d - 1].ChildrenCount <= 4);
                }
                t.Nodes[s[d].NodeIdx].BeginCallCount++;
                nodeBeginCount++;
            };
            wt.OnNodeEnd = (t, s, d) =>
            {
                Assert.AreEqual(1, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                Assert.AreEqual(1, t.Nodes[s[d].NodeIdx].BeginCallCount);
                Assert.AreEqual(0, t.Nodes[s[d].NodeIdx].EndCallCount);
                Assert.AreEqual(t.GetDepth(s[d].NodeIdx), d);
                if (checkPostValue)
                {
                    Assert.AreEqual(nodeEndCount, t.Nodes[s[d].NodeIdx].PostValue);
                }
                int expChildrenCount = d == 3 ? 0 : 4;
                Assert.AreEqual(expChildrenCount, s[d].ChildrenCount);

                nodeEndCount++;
                t.Nodes[s[d].NodeIdx].EndCallCount++;
            };
            wt.OnTreeBegin = (t) =>
            {
                Assert.AreEqual(0, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                treeBeginCount++;
            };
            wt.OnTreeEnd = (t) =>
            {
                Assert.AreEqual(1, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                treeEndCount++;
            };
            wt.Walk(tree, startNode);
            Assert.AreEqual(1, treeBeginCount);
            Assert.AreEqual(1, treeEndCount);
            Assert.AreEqual(expectedNodesCount, nodeBeginCount);
            Assert.AreEqual(expectedNodesCount, nodeEndCount);
        }

        #endregion
    }
}
