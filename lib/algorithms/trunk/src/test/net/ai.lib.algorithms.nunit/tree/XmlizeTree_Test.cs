/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.tree;
using System.Xml;
using System.Reflection;

namespace ai.lib.algorithms.tree.nunit
{
    /// <summary>
    /// Unit tests for XmlizeTree. 
    /// </summary>
    [TestFixture]
    public class XmlizeTree_Test
    {
        #region Tests

        [Test]
        public void Test_Simple()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xw = XmlWriter.Create(Console.Out, settings);
            TestNode root = new TestNode();
            CreateTestTree(ref root, 2, 2);
            root.Str = null;
            XmlizeTree<TestNode, TestNode, int, Context> xt = new XmlizeTree<TestNode, TestNode, int, Context>{Output = xw};
            xt.CompilerParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().ManifestModule.ScopeName);
            xt.ShowExpr.Add(new ExprFormatter("s[d].Node.ToString()", "{1}"));
            xt.ShowExpr.Add(new ExprFormatter("s[d].Node.Array[2]", "a;Arr2;{1}"));
            xt.ShowExpr.Add(new ExprFormatter("s[d].Node.Str", "e;Str;{1}"));
            xt.Walk(root);
            xw.Close();
            // No checks, just see it does not crash and watch text output to console.
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        public class Context : XmlizeTreeContext<TestNode, int>
        { }

        public class TestNode
        {
            public static bool TreeGetChild(TestNode tree, TestNode n, ref int i, out TestNode child)
            {
                return (i < n.Children.Length ? child = n.Children[i++] : child = null) != null;
            }
            public double Value = 5;
            public int[] Array = new int[] { 1, 2, 3 };
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
                node.Children[c].Value = c / 3.0;
                CreateTestTree(ref node.Children[c], depth - 1, childCount);
            }
        }

        #endregion
    }
}
