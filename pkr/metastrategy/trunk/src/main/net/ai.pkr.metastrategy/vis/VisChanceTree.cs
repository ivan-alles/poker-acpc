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


namespace ai.pkr.metastrategy.vis
{
    public class VisChanceTreeContext : VisPkrTreeContext<int, int>
    {
        /// <summary>
        /// Probability.
        /// </summary>
        public double Probab
        {
            set;
            get;
        }

        /// <summary>
        /// Pot shares in text form (leave empty for non-leaves).
        /// </summary>
        public string PotShares
        {
            set;
            get;
        }

        /// <summary>
        /// A convinience property, the underlying UfTree. Is helpful in expressions.
        /// </summary>
        public ChanceTree Tree
        {
            set;
            get;
        }
    }

    /// <summary>
    /// Visualizes a chance tree in UF-form by using an uni-tree adapter.
    /// <para>Usage:</para>
    /// <para>Variant 1</para>
    /// <para>1. Call static method Show() on your chance tree.</para>
    /// <para>Variant 2</para>
    /// <para>1. Create instance of the class and set parameters.</para>
    /// <para>2. Call Show() on your chance tree.</para>
    /// <para>Variant 3</para>
    /// <para>1. Create instance of the class and set parameters.</para>
    /// <para>2. Create UFToUniAdapter for your chance tree.</para>
    /// <para>3. Call Walk().</para>
    /// </summary>
    public unsafe class VisChanceTree : VisPkrTree<UFToUniAdapter, int, int, VisChanceTreeContext>
    {
        public VisChanceTree()
        {
            ShowExpr.Clear();
            ShowExpr.Add(new ExprFormatter("s[d].Id", "id:{1}"));
            ShowExpr.Add(new ExprFormatter("s[d].Probab", "\\np:{1:0.00000}"));
            ShowExpr.Add(new ExprFormatter("s[d].PotShares", "{1:0.00000}"));
        }

        public static void Show(ChanceTree ct, string fileName)
        {
            using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
            {
                VisChanceTree vis = new VisChanceTree { Output = w };
                vis.Show(ct);
            }
        }

        /// <summary>
        /// If set, the card names will be shown instead of indexes.
        /// </summary>
        public string[] CardNames
        {
            set;
            get;
        }

        public void Show(ChanceTree ct)
        {
            UFToUniAdapter adapter = new UFToUniAdapter(ct);
            Walk(adapter, 0);
        }

        protected override void OnTreeBeginFunc(UFToUniAdapter aTree, int aRoot)
        {
            ChanceTree tree = (ChanceTree)(aTree.UfTree);
            GraphAttributes.label = tree.Version.Description;
            GraphAttributes.fontsize = 20;
            base.OnTreeBeginFunc(aTree, aRoot);
        }

        protected override bool OnNodeBeginFunc(UFToUniAdapter aTree, int aNode, List<VisChanceTreeContext> stack, int depth)
        {
            VisChanceTreeContext context = stack[depth];
            ChanceTree tree = (ChanceTree)(aTree.UfTree);
            context.Tree = tree;

            context.Id = aNode.ToString();
            context.IsDealerAction = true;
            context.Position = tree.Nodes[aNode].Position;
            if (depth == 0)
            {
                context.Round = -1;
            }
            else
            {
                context.Round = stack[depth-1].Round;
            }
            // Deal to player 0 starts a new round.
            if (tree.Nodes[aNode].Position == 0)
            {
                context.Round++;
            }
            context.ActionLabel = depth == 0 ? "" : tree.Nodes[aNode].ToStrategicString(CardNames);
            context.Probab = tree.Nodes[aNode].Probab;
            // Simple implementation for two players.

            if (aTree.GetChildrenCount(aNode) == 0)
            {
                double[] potShares = new double[2];
                tree.Nodes[aNode].GetPotShare(0x3, potShares);
                context.PotShares = String.Format("\\nps0:{0:0.000}\\nps1:{1:0.000}",
                                                  potShares[0], potShares[1]);
            }
            else
            {
                context.PotShares = "";
            }

            return base.OnNodeBeginFunc(aTree, aNode, stack, depth);
        }

    }
}
