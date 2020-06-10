/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.Collections.Generic;
using System.Diagnostics;
using ai.lib.algorithms.tree;
using ai.pkr.metagame;

namespace ai.pkr.bots.neytiri
{
    class FinalizeMonteCarloData : WalkTree<ActionTree, ActionTreeNode, int>
    {
        public void Finalize(ActionTree tree, ActionTreeNode root, int pos)
        {
            _pos = pos;
            Walk(tree, root);
        }

        protected override void OnNodeEndFunc(ActionTree tree, ActionTreeNode node, List<WalkTreeContext<ActionTreeNode, int>> stack, int depth)
        {
            if (node.Children.Count == 0)
                return; // Terminal node - nothing to do.

            if (node.Children.Count == 1 && node.Children[0].ActionKind == Ak.s)
            {
                node.Value = node.Children[0].Value;
            }
            else if (node.State.CurrentActor == (1 - _pos))
            {
                // Opponent acts - merge by summing.
                node.Value = 0;
                foreach (ActionTreeNode child in node.Children)
                {
                    node.Value += child.Value;
                }
            }
            else
            {
                // We act - merge max value
                Debug.Assert(node.State.CurrentActor == _pos);
                node.Value = double.MinValue;
                foreach (ActionTreeNode child in node.Children)
                {
                    if (node.Value < child.Value)
                    {
                        node.Value = child.Value;
                    }
                }
                Debug.Assert(node.Value != double.MinValue);
            }
        }

        // Our position
        private int _pos;
    }
}