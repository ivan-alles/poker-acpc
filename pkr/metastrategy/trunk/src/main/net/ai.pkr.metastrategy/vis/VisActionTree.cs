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
    public class VisActionTreeContext : VisPkrTreeContext<int, int>
    {
        /// <summary>
        /// Bitmask of active players.
        /// </summary>
        public UInt16 ActivePlayers
        {
            set;
            get;
        }

        public StrategicState State
        {
            set;
            get;
        }

        /// <summary>
        /// A convinience property, the underlying UfTree. Is helpful in expressions.
        /// </summary>
        public ActionTree Tree
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
    public unsafe class VisActionTree : VisPkrTree<UFToUniAdapter, int, int, VisActionTreeContext>
    {
        public VisActionTree()
        {
            ShowExpr.Clear();
            ShowExpr.Add(new ExprFormatter("s[d].Id", "id:{1}"));
            ShowExpr.Add(new ExprFormatter("s[d].State.Pot", "\\np:{1}"));
            ShowExpr.Add(new ExprFormatter("s[d].ActivePlayers", "\\nap:{1:X}"));
        }

        public static void Show(ActionTree t, string fileName)
        {
            using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
            {
                VisActionTree vis = new VisActionTree { Output = w };
                vis.Show(t);
            }
        }

        public void Show(ActionTree t)
        {
            UFToUniAdapter adapter = new UFToUniAdapter(t);
            Walk(adapter, 0);
        }

        protected override void OnTreeBeginFunc(UFToUniAdapter aTree, int aRoot)
        {
            ActionTree tree = (ActionTree)(aTree.UfTree);
            GraphAttributes.label = tree.Version.Description;
            GraphAttributes.fontsize = 20;
            base.OnTreeBeginFunc(aTree, aRoot);
        }

        protected override bool OnNodeBeginFunc(UFToUniAdapter aTree, int aNode, List<VisActionTreeContext> stack, int depth)
        {
            VisActionTreeContext context = stack[depth];

            ActionTree tree = (ActionTree)(aTree.UfTree);
            context.Tree = tree;

            if (depth == 0)
            {
                context.State = new StrategicState(tree.PlayersCount);
            }
            else
            {
                context.State = stack[depth - 1].State.GetNextState(tree.Nodes[aNode]);
            }

            context.Id = aNode.ToString();
            context.IsDealerAction = false;
            context.Position = tree.Nodes[aNode].Position;
            context.Round = tree.Nodes[aNode].Round;
            context.ActivePlayers = tree.Nodes[aNode].ActivePlayers;
            context.ActionLabel = depth == 0 ? "" : tree.Nodes[aNode].ToStrategicString(null);

            return base.OnNodeBeginFunc(aTree, aNode, stack, depth);
        }

    }
}
