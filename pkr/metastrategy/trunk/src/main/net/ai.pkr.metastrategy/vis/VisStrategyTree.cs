/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.pkr.metastrategy;
using ai.pkr.metagame;
using ai.lib.algorithms;
using System.IO;
using ai.lib.algorithms.numbers;


namespace ai.pkr.metastrategy.vis
{
    public class VisStrategyTreeContext : VisPkrTreeContext<int, int>
    {
        public double Probab
        {
            set;
            get;
        }

        /// <summary>
        /// A convinience property, the underlying UfTree. Is helpful in expressions.
        /// </summary>
        public StrategyTree Tree
        {
            set;
            get;
        }
    }

    /// <summary>
    /// Visualizes a action tree in UF-form by using an uni-tree adapter.
    /// <para>Usage:</para>
    /// <para>Variant 1</para>
    /// <para>1. Call static method Show() on your action tree.</para>
    /// <para>Variant 2</para>
    /// <para>1. Create instance of the class and set parameters.</para>
    /// <para>2. Call Show() on your action tree.</para>
    /// <para>Variant 3</para>
    /// <para>1. Create instance of the class and set parameters.</para>
    /// <para>2. Create UFToUniAdapter for your action tree.</para>
    /// <para>3. Call Walk().</para>
    /// </summary>
    public unsafe class VisStrategyTree<ContextT> : VisPkrTree<UFToUniAdapter, int, int, ContextT> where ContextT : VisStrategyTreeContext, new()
    {
        public VisStrategyTree()
        {
            ShowExpr.Clear();
            ShowExpr.Add(new ExprFormatter("s[d].Id", "id:{1}"));
            ShowExpr.Add(new ExprFormatter("s[d].Probab", "\\np:{1:0.00000}"));
        }

        /// <summary>
        /// If set, the card names will be shown instead of indexes.
        /// </summary>
        public string[] CardNames
        {
            set;
            get;
        }

        public void Show(StrategyTree t)
        {
            UFToUniAdapter adapter = new UFToUniAdapter(t);
            Walk(adapter, 0);
        }

        public void Show(StrategyTree t, int root)
        {
            UFToUniAdapter adapter = new UFToUniAdapter(t);
            Walk(adapter, root);
        }

        public static void Show(StrategyTree t, string fileName)
        {
            using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
            {
                VisStrategyTree<VisStrategyTreeContext> vis = new VisStrategyTree<VisStrategyTreeContext> { Output = w };
                vis.Show(t);
            }
        }

        protected override void OnTreeBeginFunc(UFToUniAdapter aTree, int aRoot)
        {
            StrategyTree tree = (StrategyTree)(aTree.UfTree);
            GraphAttributes.label = tree.Version.Description;
            GraphAttributes.fontsize = 20;
            base.OnTreeBeginFunc(aTree, aRoot);
        }

        protected override bool OnNodeBeginFunc(UFToUniAdapter aTree, int aNode, List<ContextT> stack, int depth)
        {
            SetContext(aTree, aNode, stack, depth);

            return base.OnNodeBeginFunc(aTree, aNode, stack, depth);
        }

        /// <summary>
        /// Sets fields of the context. Override to customize view or call it in an overriden OnNodeBegin.
        /// </summary>
        protected virtual void SetContext(UFToUniAdapter aTree, int aNode, List<ContextT> stack, int depth)
        {
            ContextT context = stack[depth];
            StrategyTree tree = (StrategyTree)(aTree.UfTree);
            context.Tree = tree;

            if (depth == 0)
            {
                context.Round = -1;
            }
            else
            {
                context.Round = stack[depth - 1].Round;
            }
            // Deal action => new round.
            if (tree.Nodes[aNode].IsDealerAction)
            {
                context.Round++;
            }
            context.Id = aNode.ToString();
            context.IsDealerAction = tree.Nodes[aNode].IsDealerAction;
            context.Position = tree.Nodes[aNode].Position;
            context.Probab = tree.Nodes[aNode].IsDealerAction ? 0 : tree.Nodes[aNode].Probab;
            context.ActionLabel = depth == 0 ? "" : tree.Nodes[aNode].ToStrategicString(CardNames);
        }
    }

    public unsafe class VisStrategyTree : VisStrategyTree<VisStrategyTreeContext>
    { 
    }
}
