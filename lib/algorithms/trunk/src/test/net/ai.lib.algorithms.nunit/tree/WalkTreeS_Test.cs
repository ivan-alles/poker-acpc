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
    /// Unit tests for WalkTreeS. 
    /// </summary>
    [TestFixture]
    public class WalkTreeS_Test
    {
        #region Tests

        /// <summary>
        /// Creates a test tree and walks it using indexed child access.
        /// Tree structure is illustrated in design\walk-tree-siblings-order.gv.
        /// </summary>
        [Test]
        public void Test_SampleStructure()
        {
            TestNode[] nodes = new TestNode[13];
            for (int i = 0; i < nodes.Length; ++i)
            {
                nodes[i] = new TestNode { Value = i };
            }
            nodes[0].Children = new TestNode[] { nodes[1], nodes[2] };
            nodes[1].Children = new TestNode[] { nodes[3], nodes[4], nodes[5] };
            nodes[3].Children = new TestNode[] { nodes[6], nodes[7] };
            nodes[2].Children = new TestNode[] { nodes[8], nodes[9] };
            nodes[8].Children = new TestNode[] { nodes[10] };
            nodes[10].Children = new TestNode[] { nodes[11], nodes[12] };
            nodes[4].Children = new TestNode[] { };
            nodes[5].Children = new TestNode[] { };
            nodes[6].Children = new TestNode[] { };
            nodes[7].Children = new TestNode[] { };
            nodes[9].Children = new TestNode[] { };
            nodes[11].Children = new TestNode[] { };
            nodes[12].Children = new TestNode[] { };

            #region Expected events
            string[] expectedEvents =
                new string[] 
                {
                    "B:0",
                    "BDC:0",
                    "B:1",
                    "B:2",
                    "ADC:0",
                    "BDC:1",
                    "B:3",
                    "B:4",
                    "B:5",
                    "ADC:1",
                    "BDC:3",
                    "B:6",
                    "B:7",
                    "ADC:3",
                    "BDC:6",
                    "ADC:6",
                    "E:6",
                    "BDC:7",
                    "ADC:7",
                    "E:7",
                    "E:3",
                    "BDC:4",
                    "ADC:4",
                    "E:4",
                    "BDC:5",
                    "ADC:5",
                    "E:5",
                    "E:1",
                    "BDC:2",
                    "B:8",
                    "B:9",
                    "ADC:2",
                    "BDC:8",
                    "B:10",
                    "ADC:8",
                    "BDC:10",
                    "B:11",
                    "B:12",
                    "ADC:10",
                    "BDC:11",
                    "ADC:11",
                    "E:11",
                    "BDC:12",
                    "ADC:12",
                    "E:12",
                    "E:10",
                    "E:8",
                    "BDC:9",
                    "ADC:9",
                    "E:9",
                    "E:2",
                    "E:0"

              };
            #endregion

            List<string> actualEvents = new List<string>();

            WalkTreeS<TestNode, int> wt = new WalkTreeS<TestNode, int>();
            wt.OnNodeBegin = (t, n, s, d) =>
            {
                string ev = string.Format("B:{0}", n.Value);
                actualEvents.Add(ev);
                //Console.WriteLine(ev);
                return true;
            };
            wt.BeforeDirectChildren = (t, n, s, d) =>
            {
                string ev = string.Format("BDC:{0}", n.Value);
                actualEvents.Add(ev);
                //Console.WriteLine(ev);
            };
            wt.AfterDirectChildren = (t, n, s, d) =>
            {
                string ev = string.Format("ADC:{0}", n.Value);
                actualEvents.Add(ev);
                //Console.WriteLine(ev);
            };
            wt.OnNodeEnd = (t, n, s, d) =>
            {
                string ev = string.Format("E:{0}", n.Value);
                actualEvents.Add(ev);
                //Console.WriteLine(ev);
            };
            wt.Walk(nodes[0]);
            Assert.AreEqual(expectedEvents, actualEvents.ToArray());
        }

        /// <summary>
        /// Creates a test tree and walks it using indexed child access.
        /// </summary>
        [Test]
        public void Test_WalkTreeS_Index()
        {
            WalkTreeS<TestNode, int> wt = new WalkTreeS<TestNode, int>();
            DoWalkTreeS(wt);
        }

        /// <summary>
        /// Creates a test tree and walks it using indexed child access.
        /// </summary>
        [Test]
        public void Test_WalkTreeS_Enum()
        {
            WalkTreeS<TestNode, IEnumerator> wt = new WalkTreeS<TestNode, IEnumerator>();

            wt.GetChild = (TestNode t, TestNode n, ref IEnumerator i, out TestNode c) =>
                              {
                                  if (i == null) i = n.Children.GetEnumerator();
                                  c = i.MoveNext() ? (TestNode)i.Current : null;
                                  return c != null;
                              };

            DoWalkTreeS(wt);
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public unsafe void Benchmark_WalkTreeS_Index()
        {
            TestNode root = new TestNode();
            int depth = 24;
            CreateTestTree(ref root, depth - 1, 2);
            BenchmarkTreeWalker tw = new BenchmarkTreeWalker();

            DateTime startTime = DateTime.Now;

            tw.Walk(root);

            double time = (DateTime.Now - startTime).TotalSeconds;

            Assert.AreEqual((1 << depth) - 1, tw.NodeCount);

            Console.WriteLine("Node count {0:###,###,###}, checksum {1}, time {2} s, {3:###,###,###} n/s",
                tw.NodeCount, tw.CheckSum, time, tw.NodeCount / time);
        }

        [Test]
        [Category("Benchmark")]
        public unsafe void Benchmark_WalkTreeS_Enum()
        {
            TestNode root = new TestNode();
            int depth = 24;
            CreateTestTree(ref root, depth - 1, 2);
            BenchmarkTreeWalkerEnum tw = new BenchmarkTreeWalkerEnum();

            DateTime startTime = DateTime.Now;

            tw.Walk(root);

            double time = (DateTime.Now - startTime).TotalSeconds;

            Assert.AreEqual((1 << depth) - 1, tw.NodeCount);

            Console.WriteLine("Node count {0:###,###,###}, checksum {1}, time {2} s, {3:###,###,###} n/s",
                tw.NodeCount, tw.CheckSum, time, tw.NodeCount / time);
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

        class BenchmarkTreeWalker : WalkTreeS<TestNode, int>
        {
            public int NodeCount = 0;
            public int CheckSum = 0;

            protected override bool OnNodeBeginFunc(TestNode tree, TestNode node, List<WalkTreeSContext<TestNode, int>> stack, int depth)
            {
                NodeCount++;
                CheckSum += node.Value;
                return true;
            }
        }

        class BenchmarkTreeWalkerEnum : WalkTreeS<TestNode, IEnumerator>
        {
            public int NodeCount = 0;
            public int CheckSum = 0;

            public BenchmarkTreeWalkerEnum()
            {
                GetChild = (TestNode t, TestNode n, ref IEnumerator i, out TestNode c) =>
                {
                    if (i == null) i = n.Children.GetEnumerator();
                    c = i.MoveNext() ? (TestNode)i.Current : null;
                    return c != null;
                };
            }

            protected override bool OnNodeBeginFunc(TestNode tree, TestNode node, List<WalkTreeSContext<TestNode, IEnumerator>> stack, int depth)
            {
                NodeCount++;
                CheckSum += node.Value;
                return true;
            }
        }

        /// Skips nodes with Value == 0, first walk with OnNodeBegin, 2nd - with PruneIf.
        /// Verifies that all neccessary function are called, etc.
        private void DoWalkTreeS<IteratorT>(WalkTreeS<TestNode, IteratorT> wt)
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
                n.Depth = d+1;
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
            foreach (TestNode child in node.Children)
            {
                VerifyTree(child, node, depth + 1, ref nodeCount, usePruneIf);
            }
        }

        #endregion
    }
}
