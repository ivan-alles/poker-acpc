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
    /// Unit tests for CompareTrees. 
    /// </summary>
    [TestFixture]
    public class CompareTrees_Test
    {
        #region Tests

        [Test]
        public void Test_Equal()
        {
            int nodeCount;
            TestNode1 root1;
            nodeCount = 0;
            CreateTestTree1(out root1, 4, 3, ref nodeCount);
            TestNode2 root2;
            nodeCount = 0;
            CreateTestTree2(out root2, 4, 3, ref nodeCount);

            CompareTrees<TestNode1, TestNode1, int, TestNode2, TestNode2, int> comp =
                new CompareTrees<TestNode1, TestNode1, int, TestNode2, TestNode2, int>();
            bool result = comp.Compare(root1, root1, root2, root2, (t1, n1, t2, n2) => n1.IntValue == n2.DoubleValue);
            Assert.IsTrue(result);
            Assert.AreEqual(CompareTrees.ResultKind.Equal, comp.Result);

            // Test helper functions as well.
            result = CompareTrees<int, int>.Compare(root1, root1, root2, root2, (t1, n1, t2, n2) => n1.IntValue == n2.DoubleValue);
            Assert.IsTrue(result);

            result = CompareTrees<int, int>.Compare(root1, root1, root2, root2);
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_StructureDiffers()
        {
            int nodeCount;
            TestNode1 root1;
            nodeCount = 0;
            CreateTestTree1(out root1, 4, 2, ref nodeCount);
            TestNode2 root2;
            nodeCount = 0;
            CreateTestTree2(out root2, 4, 2, ref nodeCount);

            Array.Resize(ref root2.Children, 1); // Change the structure

            CompareTrees<TestNode1, TestNode1, int, TestNode2, TestNode2, int> comp =
                new CompareTrees<TestNode1, TestNode1, int, TestNode2, TestNode2, int>();
            bool result = comp.Compare(root1, root1, root2, root2, (t1, n1, t2, n2) => n1.IntValue == n2.DoubleValue);
            Assert.IsFalse(result);
            Assert.AreEqual(CompareTrees.ResultKind.StructureDiffers, comp.Result);
            Assert.AreEqual(8, comp.DiffersAt);

            // Test helper functions as well.
            result = CompareTrees<int, int>.Compare(root1, root1, root2, root2, (t1, n1, t2, n2) => n1.IntValue == n2.DoubleValue);
            Assert.IsFalse(result);

            result = CompareTrees<int, int>.Compare(root1, root1, root2, root2);
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_ValueDiffers()
        {
            int nodeCount;
            TestNode1 root1;
            nodeCount = 0;
            CreateTestTree1(out root1, 4, 2, ref nodeCount);
            TestNode2 root2;
            nodeCount = 0;
            CreateTestTree2(out root2, 4, 2, ref nodeCount);

            root2.Children[1].Children[0].DoubleValue = -1; // Change the value

            CompareTrees<TestNode1, TestNode1, int, TestNode2, TestNode2, int> comp =
                new CompareTrees<TestNode1, TestNode1, int, TestNode2, TestNode2, int>();
            bool result = comp.Compare(root1, root1, root2, root2, (t1, n1, t2, n2) => n1.IntValue == n2.DoubleValue);
            Assert.IsFalse(result);
            Assert.AreEqual(CompareTrees.ResultKind.ValueDiffers, comp.Result);
            Assert.AreEqual(9, comp.DiffersAt);

            // Test helper functions as well.
            result = CompareTrees<int, int>.Compare(root1, root1, root2, root2, (t1, n1, t2, n2) => n1.IntValue == n2.DoubleValue);
            Assert.IsFalse(result);

            result = CompareTrees<int, int>.Compare(root1, root1, root2, root2);
            Assert.IsFalse(result);
        }

        #endregion

        #region Benchmarks

        #endregion

        #region Implementation

        class TestNode1
        {
            public static bool TreeGetChild(TestNode1 tree, TestNode1 n, ref int i, out TestNode1 child)
            {
                return (i < n.Children.Length ? child = n.Children[i++] : child = null) != null;
            }

            public int IntValue;
            public TestNode1[] Children;

            public override bool Equals(object obj)
            {
                if(obj is TestNode1)
                {
                    return IntValue == ((TestNode1) obj).IntValue;
                }
                if (obj is TestNode2)
                {
                    return IntValue == ((TestNode2)obj).DoubleValue;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        class TestNode2
        {
            public static bool TreeGetChild(TestNode2 tree, TestNode2 n, ref int i, out TestNode2 child)
            {
                return (i < n.Children.Length ? child = n.Children[i++] : child = null) != null;
            }

            public double DoubleValue;
            public TestNode2[] Children;
        }

        void CreateTestTree1(out TestNode1 node, int depth, int childCount, ref int nodeCount)
        {
            node = new TestNode1();
            node.IntValue = nodeCount++;
            
            if(depth == 1)
            {
                // Terminal node
                node.Children = new TestNode1[0];
                return;
            }

            node.Children = new TestNode1[childCount];
            for (int c = 0; c < childCount; ++c)
            {
                CreateTestTree1(out node.Children[c], depth - 1, childCount, ref nodeCount);
            }
        }

        void CreateTestTree2(out TestNode2 node, int depth, int childCount, ref int nodeCount)
        {
            node = new TestNode2();
            node.DoubleValue = nodeCount++;

            if (depth == 1)
            {
                // Terminal node
                node.Children = new TestNode2[0];
                return;
            }

            node.Children = new TestNode2[childCount];
            for (int c = 0; c < childCount; ++c)
            {
                CreateTestTree2(out node.Children[c], depth - 1, childCount, ref nodeCount);
            }
        }

        #endregion
    }
}
