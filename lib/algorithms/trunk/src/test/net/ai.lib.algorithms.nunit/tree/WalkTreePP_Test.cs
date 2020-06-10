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
    /// Unit tests for WalkTreePP. 
    /// </summary>
    [TestFixture]
    public class WalkTreePP_Test
    {
        #region Tests

        /// <summary>
        /// Creates a test tree and walks it using indexed child access.
        /// </summary>
        [Test]
        public void Test_WalkTreePP_Index()
        {
            WalkTreePP<TestNode, int> wt = new WalkTreePP<TestNode, int>();
            DoWalkTreePP(wt);
        }

        /// <summary>
        /// Creates a test tree and walks it using indexed child access.
        /// </summary>
        [Test]
        public void Test_WalkTreePP_Enum()
        {
            WalkTreePP<TestNode, IEnumerator> wt = new WalkTreePP<TestNode, IEnumerator>();

            wt.GetChild = (TestNode t, TestNode n, ref IEnumerator i, out TestNode c) =>
                              {
                                  if (i == null) i = n.Children.GetEnumerator();
                                  c = i.MoveNext() ? (TestNode)i.Current : null;
                                  return c != null;
                              };
            
            DoWalkTreePP(wt);
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public unsafe void Benchmark_WalkTreePP_Index()
        {
            TestNode root = new TestNode();
            int depth = 24;
            CreateTestTree(ref root, depth - 1, 2);
            BenchmarkTreeWalker tw = new BenchmarkTreeWalker();

            DateTime startTime = DateTime.Now;

            tw.Walk(root);

            double time = (DateTime.Now - startTime).TotalSeconds;

            Assert.AreEqual((1 << depth) - 1, tw.NodeCount);

            Console.WriteLine("Node count {0:###,###,###}, time {1} s, {2:###,###,###} n/s",
                tw.NodeCount, time, tw.NodeCount / time);
        }

        [Test]
        [Category("Benchmark")]
        public unsafe void Benchmark_WalkTreePP_Enum()
        {
            TestNode root = new TestNode();
            int depth = 24;
            CreateTestTree(ref root, depth - 1, 2);
            BenchmarkTreeWalkerEnum tw = new BenchmarkTreeWalkerEnum();

            DateTime startTime = DateTime.Now;

            tw.Walk(root);

            double time = (DateTime.Now - startTime).TotalSeconds;

            Assert.AreEqual((1 << depth) - 1, tw.NodeCount);

            Console.WriteLine("Node count {0:###,###,###}, time {1} s, {2:###,###,###} n/s",
                tw.NodeCount, time, tw.NodeCount / time);
        }
        #endregion

        #region Implementation

        class TestNode
        {
            public static bool TreeGetChild(TestNode tree, TestNode n, ref int i, out TestNode child)
            {
                return (i < n.Children.Length ? child = n.Children[i++] : child = null) != null;
            }

            public int Value;
            public int BeginCallCount;
            public int EndCallCount;
            public int Depth;
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

        class BenchmarkTreeWalker : WalkTreePP<TestNode, int>
        {
            public int NodeCount = 0;

            protected override bool OnNodeBeginFunc(TestNode tree, TestNode node, List<WalkTreePPContext<TestNode, int>> stack, int depth)
            {
                NodeCount++;
                return true;
            }
        }

        class BenchmarkTreeWalkerEnum : WalkTreePP<TestNode, IEnumerator>
        {
            public int NodeCount = 0;

            public BenchmarkTreeWalkerEnum()
            {
                GetChild = (TestNode t, TestNode n, ref IEnumerator i, out TestNode c) =>
                {
                    if (i == null) i = n.Children.GetEnumerator();
                    c = i.MoveNext() ? (TestNode)i.Current : null;
                    return c != null;
                };
            }

            protected override bool OnNodeBeginFunc(TestNode tree, TestNode node, List<WalkTreePPContext<TestNode, IEnumerator>> stack, int depth)
            {
                NodeCount++;
                return true;
            }
        }

        /// Skips nodes with Value == 0, first walk with OnNodeBegin, 2nd - with PruneIf.
        /// Verifies that all neccessary function are called, etc.
        private void DoWalkTreePP<IteratorT>(WalkTreePP<TestNode, IteratorT> wt)
        {
            TestNode root = new TestNode();
            CreateTestTree(ref root, 3, 4);
            root.Value = 1000;

            int treeBeginCount = 0;
            int treeEndCount = 0;
            int nodeBeginCount = 0;
            int nodeEndCount = 0;

            wt.OnNodeBegin = (t, n, s, d) =>
            {
                Assert.AreEqual(1, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                Assert.AreEqual(0, n.BeginCallCount);
                Assert.AreEqual(0, n.EndCallCount);
                n.Depth = d;
                n.BeginCallCount ++;
                nodeBeginCount++;
                return n.Value > 0;
            };
            wt.OnNodeEnd = (t, n, s, d) =>
            {
                Assert.AreEqual(1, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                Assert.AreEqual(1, n.BeginCallCount);
                Assert.AreEqual(0, n.EndCallCount);
                Assert.AreEqual(n.Depth, d);
                nodeEndCount++;
                n.EndCallCount ++;
            };
            wt.OnTreeBegin = (t, n) =>
            {
                Assert.AreEqual(0, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                treeBeginCount++;
            };
            wt.OnTreeEnd = (t, n) =>
            {
                Assert.AreEqual(1, treeBeginCount);
                Assert.AreEqual(0, treeEndCount);
                treeEndCount++;
            };
            wt.Walk(root);
            Assert.AreEqual(1, treeBeginCount);
            Assert.AreEqual(1, treeEndCount);
            Assert.AreEqual(1 + 4 + 3 * 4 + 3 * 3 * 4, nodeBeginCount);
            Assert.AreEqual(1 + 3 + 3 * 3 + 3 * 3 * 3, nodeEndCount);

            int nodeCount = 0;
            VerifyTree(root, null, 0, ref nodeCount, false);
            Assert.AreEqual(1 + 4 + 4 * 4 + 4 * 4 * 4, nodeCount);

            // Now do pruning with PruneIf instead of OnNodeBegin.
            root = new TestNode();
            CreateTestTree(ref root, 3, 4);
            root.Value = 1000;
            wt.PruneIf = n => n.Value == 0;
            treeBeginCount = 0;
            treeEndCount = 0;
            nodeBeginCount = 0;
            nodeEndCount = 0;
            wt.Walk(root);
            Assert.AreEqual(1, treeBeginCount);
            Assert.AreEqual(1, treeEndCount);
            Assert.AreEqual(1 + 3 + 3 * 3 + 3 * 3 * 3, nodeBeginCount);
            Assert.AreEqual(1 + 3 + 3 * 3 + 3 * 3 * 3, nodeEndCount);

            nodeCount = 0;
            VerifyTree(root, null, 0, ref nodeCount, true);
            Assert.AreEqual(1 + 4 + 4 * 4 + 4 * 4 * 4, nodeCount);
        }


        void VerifyTree(TestNode node, TestNode parent, int depth, ref int nodeCount, bool usePruneIf)
        {
            nodeCount++;
            if (parent != null)
            {
                if(parent.EndCallCount == 0)
                {
                    Assert.AreEqual(0, node.BeginCallCount);
                    Assert.AreEqual(0, node.EndCallCount);
                    Assert.AreEqual(0, node.Depth);
                }
                else if (node.Value == 0)
                {
                    if (usePruneIf)
                    {
                        Assert.AreEqual(0, node.BeginCallCount);
                        Assert.AreEqual(0, node.Depth);
                    }
                    else
                    {
                        Assert.AreEqual(1, node.BeginCallCount);
                        Assert.AreEqual(depth, node.Depth);
                    }
                    Assert.AreEqual(0, node.EndCallCount);
                }
                else
                {
                    Assert.AreEqual(1, node.BeginCallCount);
                    Assert.AreEqual(1, node.EndCallCount);
                    Assert.AreEqual(depth, node.Depth);
                }
            }
            else
            {
                Assert.AreEqual(1, node.BeginCallCount);
                Assert.AreEqual(1, node.EndCallCount);
                Assert.AreEqual(depth, node.Depth);
            }
            foreach(TestNode child in node.Children)
            {
                VerifyTree(child, node, depth + 1, ref nodeCount, usePruneIf);
            }
        }

        #endregion
    }
}
