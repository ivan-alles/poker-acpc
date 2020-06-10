/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using ai.pkr.metastrategy;

namespace ai.pkr.bots.neytiri
{
    public class ActionTreeVisualizerContext: VisPokerTreeContext<ActionTreeNode, int>
    {
        public string NodeId;
        public double Count;

        public override string ToPathString()
        {
            return Node.ActionKind.ToString();
        }
    }

    public class ActionTreeVisualizer : VisPokerTree<ActionTree, ActionTreeNode, int, ActionTreeVisualizerContext>
    {

        /// <summary>
        /// Show up to N buckets.
        /// </summary>
        public int ShowBuckets
        {
            set;
            get;
        }

        public List<ActionTreeNode> StrategyPath
        {
            set;
            get;
        }

        protected override bool OnNodeBeginFunc(ActionTree tree, ActionTreeNode node, List<ActionTreeVisualizerContext> stack, int depth)
        {
            if (!base.OnNodeBeginFunc(tree, node, stack, depth))
                return false;
            ActionTreeVisualizerContext context = stack[depth];
            context.NodeId = node.Id.ToString();
            context.Action = new PokerAction(node.ActionKind, node.State.LastActor, -1, "");
            if(node.ActionKind == Ak.r)
            {
                context.Action.Amount = tree.GameDef.BetStructure[node.State.Round];
            }
            context.State = node.State;
            context.Count = node.Value;
            return true;
        }

        protected override string GetNodeLabel(ActionTree tree, ActionTreeNode node, List<ActionTreeVisualizerContext> stack, int depth)
        {
            string buckets = "";
            if (ShowBuckets > 0)
            {
                int bucketCount = Math.Min(ShowBuckets, node.OppBuckets.Counts.Length);
                // Show for postflop the last N because preflop the first are strong and postflop vice versa
                int start = node.State.Round == 0 ? 0 : node.OppBuckets.Counts.Length - bucketCount;
                for (int b = start; b < start + bucketCount; ++b)
                {
                    buckets += "\\n" + String.Format("{0}:{1}", b, node.OppBuckets.Counts[b]);
                }
                buckets += "\\n" + String.Format("T:{0}", node.OppBuckets.Total);
            }
            string label = base.GetNodeLabel(tree, node, stack, depth) + buckets;
            return label;
        }

    }
}
