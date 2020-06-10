/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.pkr.metagame;
using ai.lib.algorithms.numbers;
using ai.pkr.metastrategy;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Converts a strategy stored with conditional probabilities to absolute probabilities.
    /// The strategy is overwritten.
    /// </summary>
    public unsafe class ConvertCondToAbs : WalkUFTreePP<StrategyTree, ConvertCondToAbs.Context>
    {
        public class Context : WalkUFTreePPContext
        {
            /// <summary>
            /// Own probability of a node calculated from parent nodes.
            /// </summary>
            internal double Probability;
        }

        public int HeroPosition
        {
            set;
            get;
        }

        public static void Convert(StrategyTree strategy, int heroPos)
        {
            ConvertCondToAbs converter = new ConvertCondToAbs
                                             {
                                                 HeroPosition = heroPos,
                                             };
            // Skip root and blinds
            converter.Walk(strategy, strategy.PlayersCount);
        }

        protected override void OnNodeBeginFunc(StrategyTree tree, Context[] stack, int depth)
        {
            Int64 n = stack[depth].NodeIdx;
            if (depth == tree.PlayersCount)
            {
                stack[depth].Probability = 1.0;
            }
            else
            {
                stack[depth].Probability = stack[depth - 1].Probability;
                
                if (tree.Nodes[n].IsPlayerAction(HeroPosition))
                {
                    double probab = tree.Nodes[n].Probab;
                    stack[depth].Probability *= probab;
                    tree.Nodes[n].Probab = stack[depth].Probability;
                }
            }
        }
    }
}