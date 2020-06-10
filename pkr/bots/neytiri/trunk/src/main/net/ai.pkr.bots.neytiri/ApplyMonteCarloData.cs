/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.Diagnostics;
using ai.lib.algorithms.tree;
using System.Collections.Generic;
using ai.pkr.metagame;

namespace ai.pkr.bots.neytiri
{
    class ApplyMonteCarloData 
    {
        public ApplyMonteCarloData()
        {
        }

        public void ApplyData(ActionTreeNode root, MonteCarloData [] mc, int ourPos,
                              int showdownValue, double strategyFactor)
        {
            _mc = mc;
            _ourPos = ourPos;
            _showdownValue = showdownValue;
            root.StrategyFactor = strategyFactor;
            TraverseSubtree(root, null);
        }

        protected bool TraverseSubtree(ActionTreeNode node, ActionTreeNode parent)
        {
            if (!OnNodeBegin(node, parent))
                return false;
            ProcessChildren(node);
            return true;
        }

        protected bool OnNodeBegin(ActionTreeNode node, ActionTreeNode parent)
        {
            if (node.ActionKind == Ak.s)
            {
                Debug.Assert(node.State.LastActor == -1);

                // This is a point where a bucket is transformed to the next round's buckets.
                // Here we do not have know the probability of transformation from 
                // mc[r].bucket -> mc[r+1].bucket, because this probability i
                // is inherently taken into account by Monte-Carlo method.
            }
            else if (parent != null && parent.State.CurrentActor == 1 - _ourPos && !parent.State.IsDealerActing)
            {
                // Opponent have acted - apply strategic probability.
                Debug.Assert(node.State.Round == parent.State.Round);
                int bucket = _mc[node.State.Round].bucket;
                int parentFreq = parent.OppBuckets.Counts[bucket];
                int nodeFreq = node.OppBuckets.Counts[bucket];
                if (parentFreq != 0)
                    node.StrategyFactor *= (double)nodeFreq / parentFreq;
                else
                {
                    Debug.Assert(nodeFreq == 0);
                    node.StrategyFactor = 0;
                }
            }

            if(node.Children.Count == 0)
            {
                // Terminal node.
                Debug.Assert(node.State.IsGameOver);

                double value;

                if (node.State.IsShowdownRequired)
                {
                    // Noone folded - do showdown.
                    value = node.State.Pot/2*_showdownValue;
                }
                else if (node.State.Players[_ourPos].IsFolded)
                {
                    // We folded
                    value = - node.State.Players[_ourPos].InPot;
                }
                else 
                {
                    // Opponent folded
                    Debug.Assert(node.State.Players[1 - _ourPos].IsFolded);
                    value = node.State.Players[1 - _ourPos].InPot;
                }
                value *= node.StrategyFactor;
                node.Value += value;
            }
            return node.StrategyFactor > 0;
        }

        // Use custom implementation for optitmization reasons.
        protected void ProcessChildren(ActionTreeNode node)
        {
            for (int c = 0; c < node.Children.Count; ++c)
            {
                node.Children[c].StrategyFactor = node.StrategyFactor;
                TraverseSubtree(node.Children[c], node);
            }
        }

        private MonteCarloData[] _mc;
        private int _ourPos;
        private int _showdownValue;
    }
}