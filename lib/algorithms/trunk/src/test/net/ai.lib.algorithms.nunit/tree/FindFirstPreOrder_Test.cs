/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.tree;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for FindFirstPreOrder 
    /// </summary>
    [TestFixture]
    public class FindFirstPreOrder_Test
    {
        #region Tests

        [Test]
        public void Test_FindFirstPreorder()
        {
            TestNode root = new TestNode();
            CreateTestTree(ref root, 4, 3);

            TestNode node = FindFirstPreOrder<int>.Find(root,
                                                               n => n.Value == 1000);

            Assert.IsNull(node);

            TestNode mark = root.Children[1].Children[2].Children[1];
            mark.Value = 1000;

            // Mark another node further in the tree too.
            root.Children[2].Value = 1000;

            node = FindFirstPreOrder<int>.Find(root, n => n.Value == 1000);
            Assert.AreEqual(mark, node);
        }

        #endregion

       

        #region Benchmarks
        [Test]
        [Category("Benchmark")]
        public void Bencmark_FindFirstPreOrder()
        {
            TestNode root = new TestNode();
            int depth = 5;
            CreateTestTree(ref root, depth - 1, 3);

            int repetitions = 100000;

            TestNode mark = root.Children[1].Children[2].Children[1].Children[0];

            // Force jitting.
            TestNode node = FindFirstPreOrder<int>.Find(root, n => n.Value == 1000);

            //
            // Search using static method.
            //
            int valueToSearch = 1000;
            mark.Value = valueToSearch;
            DateTime startTime = DateTime.Now;
            for (int r = 0; r < repetitions; ++r)
            {
                node = FindFirstPreOrder<int>.Find(root, n => n.Value == valueToSearch);
                node.Value++;
                valueToSearch++;
            }
            double time = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Static method: repetitions {0:###,###,###}, time {1} s, {2:###,###,###} searches/s",
                repetitions, time, repetitions/ time);
            //
            // Search using an instance.
            //
            valueToSearch = 1000;
            mark.Value = valueToSearch;
            FindFirstPreOrder<TestNode, TestNode, int> finder = new FindFirstPreOrder<TestNode, TestNode, int> 
            { Match = n => n.Value == valueToSearch };

            startTime = DateTime.Now;
            for (int r = 0; r < repetitions; ++r)
            {
                finder.Find(root, root);
                finder.Result.Value++;
                valueToSearch++;
            }
            time = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Instance     : repetitions {0:###,###,###}, time {1} s, {2:###,###,###} searches/s",
                repetitions, time, repetitions / time);
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
            public TestNode[] Children;
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
