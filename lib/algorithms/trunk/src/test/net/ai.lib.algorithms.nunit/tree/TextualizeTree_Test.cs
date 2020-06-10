/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.tree;
using System.Reflection;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for TextualizeTree. 
    /// </summary>
    [TestFixture]
    public class TextualizeTree_Test
    {
        #region Tests

        [Test]
        public void Test_ShowExpr()
        {
            TestNode root = new TestNode();
            CreateTestTree(ref root, 2, 2);
            root.Str = null;
            TextualizeTree<TestNode, TestNode, int, Context> vis = new TextualizeTree<TestNode, TestNode, int, Context>();
            vis.CompilerParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().ManifestModule.ScopeName);
            vis.ShowExpr.Add(new ExprFormatter("s[d].Node.Value", " {0}: {1:0.###}"));
            vis.ShowExpr.Add(new ExprFormatter("s[d].Node.Array[2]", " {0}: {1}"));
            vis.ShowExpr.Add(new ExprFormatter("s[d].Node.Str", " {0}: '{1}'"));
            vis.ShowExpr.Add(new ExprFormatter("t", " {0}: '{1}'"));
            vis.ShowExpr.Add(new ExprFormatter("c", " {0}: '{1}'"));
            vis.Walk(root);
            // No checks, just see it does not crash.
        }

        [Test]
        public void Test_ShowExprFromString()
        {
            TextualizeTree<TestNode, TestNode, int, Context> vis = new TextualizeTree<TestNode, TestNode, int, Context>();
            vis.ShowExprFromString(new string[] {"s[d].Node.Value", "s[d].Node.Array[2];", "s[d].Node;n:{1}", "1;1"});
            
            Assert.AreEqual("s[d].Node.Value", vis.ShowExpr[0].Expr);
            Assert.AreEqual("{0}:{1}", vis.ShowExpr[0].Format);

            Assert.AreEqual("s[d].Node.Array[2]", vis.ShowExpr[1].Expr);
            Assert.AreEqual("{0}:{1}", vis.ShowExpr[1].Format);

            Assert.AreEqual("s[d].Node", vis.ShowExpr[2].Expr);
            Assert.AreEqual("n:{1}", vis.ShowExpr[2].Format);

            Assert.AreEqual("1", vis.ShowExpr[3].Expr);
            Assert.AreEqual("1", vis.ShowExpr[3].Format);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        public class Context: TextualizeTreeContext<TestNode, int>
        {}

        public class TestNode
        {
            public static bool TreeGetChild(TestNode tree, TestNode n, ref int i, out TestNode child)
            {
                return (i < n.Children.Length ? child = n.Children[i++] : child = null) != null;
            }
            public double Value = 5;
            public int[] Array = new int[] {1, 2, 3};
            public string Str = "s";
            public TestNode[] Children;

            public override string ToString()
            {
                return "TestNode";
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
                node.Children[c].Value = c/3.0;
                CreateTestTree(ref node.Children[c], depth - 1, childCount);
            }
        }

        #endregion
    }
}
