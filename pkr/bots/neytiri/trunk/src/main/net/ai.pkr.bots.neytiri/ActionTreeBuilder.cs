/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.pkr.metastrategy;

namespace ai.pkr.bots.neytiri
{
    public class ActionTreeBuilderContext : WalkTreeContext<GenNode, int>
    {
        public ActionTreeNode AtNode;
    }

    class ActionTreeBuilder : WalkTree<GenTree, GenNode, int, ActionTreeBuilderContext>
    {

        /// <summary>
        /// Build a tree for position pos.
        /// </summary>
        public void Build(ActionTree atTree, int pos)
        {
            //GetChild = (GenTree t, DeckGenNode n, ref int i, out DeckGenNode c) =>
            //               {
            //                   GenNode c1;
            //                   GenTree.TreeGetChild(t, n, ref i, out c)
            //               };

            _pos = pos;

            _atTree = atTree;
            _atTree.Positions[pos] = new ActionTreeNode();

            GenTree genTree = new GenTree {GameDef = _atTree.GameDef, Kind = GenTree.TreeKind.ActionTree};
            DeckGenNode genRoot = new DeckGenNode(genTree, 2);

            NodesCount = 0;
            Walk(genTree, genRoot);
        }

        /// <summary>
        /// Contains number of nodes traversed by Build().
        /// </summary>
        public int NodesCount
        {
            protected set;
            get;
        }

        protected override bool OnNodeBeginFunc(GenTree tree, GenNode node, List<ActionTreeBuilderContext> stack, int depth)
        {
            base.OnNodeBeginFunc(tree, node, stack, depth);
            ActionTreeNode atNode;
            if(depth == 0)
            {
                atNode = _atTree.Positions[_pos];
            }
            else
            {
                atNode = new ActionTreeNode();
                stack[depth - 1].AtNode.Children.Add(atNode);
            }
            stack[depth].AtNode = atNode;
            atNode.State = node.State;
            atNode.ActionKind = node.Action.Kind;
            atNode.Id = NodesCount;
            NodesCount++;

            GameState gs = atNode.State;

            if (gs.IsGameOver)
            {
                if (gs.Players[1 - _pos].IsFolded)
                {
                    atNode.OppBuckets = new Buckets(0, 0); // If we fold, it is never used
                }
                else
                {
                    atNode.OppBuckets = new Buckets(_atTree.Bucketizer.BucketCount[gs.Round], 0);
                }
                return false;
            }
            atNode.OppBuckets = new Buckets(depth > 0 ? _atTree.Bucketizer.BucketCount[gs.Round] : 0, 0);
            return true;
        }

        private int _pos;
        ActionTree _atTree;
    }
}
