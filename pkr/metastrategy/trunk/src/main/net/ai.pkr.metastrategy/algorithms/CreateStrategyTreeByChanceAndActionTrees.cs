/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.pkr.metagame;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Create strategy tree by action tree and player chance tree.
    /// Cards in deal nodes will be in the same order as in the player chance tree.
    /// Probabilities are initialized with 0.
    /// </summary>
    public unsafe class CreateStrategyTreeByChanceAndActionTrees
    {
        class Creator : WalkChanceAndActionTrees<Creator.Context>
        {
            public class Context : WalkChanceAndActionTreesContext
            {
                public int DepthDelta = 1;
            }

            public StrategyTree StrategyTree;
            public Int64 NodeCount;

            protected override void OnNodeBegin(Context[] stack, int depth)
            {
                Context context = stack[depth];
                Int64 nodeIdx = context.NodeIdx[(int)context.TreeKind];
                if (depth > 0)
                {
                    context.DepthDelta = stack[depth - 1].DepthDelta;
                }
                //if (context.TreeKind == TreeKind.Chance && nodeIdx == 0)
                //{
                //    // Skip the root of the chance tree.
                //    return;
                //}
                //if (context.TreeKind == TreeKind.Action && nodeIdx > 0 && ActionTree.Nodes[nodeIdx].Round == -1)
                //{
                //    // Skip blinds.
                //    context.DepthDelta--;
                //    return;
                //}

                int strategyDepth = depth + context.DepthDelta;
                if (StrategyTree != null)
                {
                    StrategyTree.SetDepth(NodeCount, (byte)strategyDepth);
                    StrategyTree.Nodes[NodeCount].Probab = 0;
                    if(context.TreeKind == TreeKind.Chance)
                    {
                        StrategyTree.Nodes[NodeCount].IsDealerAction = true;
                        StrategyTree.Nodes[NodeCount].Position = PlayerChanceTree.Nodes[nodeIdx].Position;
                        StrategyTree.Nodes[NodeCount].Card = PlayerChanceTree.Nodes[nodeIdx].Card;
                    }
                    else
                    {
                        StrategyTree.Nodes[NodeCount].IsDealerAction = false;
                        StrategyTree.Nodes[NodeCount].Position = ActionTree.Nodes[nodeIdx].Position;
                        StrategyTree.Nodes[NodeCount].Amount = ActionTree.Nodes[nodeIdx].Amount;
                    }
                }
                NodeCount++;
            }
        }


        public StrategyTree Create(ChanceTree playerChanceTree, ActionTree  actionTree)
        {
            Creator c = new Creator{ActionTree = actionTree, PlayerChanceTree = playerChanceTree};
            // Start from 1 because the root is skipped.
            c.NodeCount = 1;
            c.Walk();
            c.StrategyTree = new StrategyTree(c.NodeCount);
            c.NodeCount = 1;
            c.Walk();
            // Set root
            c.StrategyTree.SetDepth(0, 0);
            c.StrategyTree.Nodes[0].IsDealerAction = false;
            c.StrategyTree.Nodes[0].Position = actionTree.Nodes[0].Position;
            c.StrategyTree.Nodes[0].Amount = 0;
            c.StrategyTree.Nodes[0].Probab = 0;

            c.StrategyTree.Version.Description = String.Format("Strategy tree from {0}, {1}", 
                actionTree.Version.Description,
                playerChanceTree.Version.Description);

            return c.StrategyTree;
        }

        public static StrategyTree CreateS(ChanceTree playerChanceTree, ActionTree actionTree)
        {
            CreateStrategyTreeByChanceAndActionTrees c = new CreateStrategyTreeByChanceAndActionTrees();
            return c.Create(playerChanceTree, actionTree);
        }

    }
}
