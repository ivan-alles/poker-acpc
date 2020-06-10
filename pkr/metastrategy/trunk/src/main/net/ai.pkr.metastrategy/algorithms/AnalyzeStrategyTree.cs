/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;
using System.IO;
using ai.pkr.metagame;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Analyzes a strategy tree. The results of comparison are stored in properties and can
    /// also be printed in verbose mode.
    /// <para>Usage scenarios: </para>
    /// <para>1. Set StrategyTree, IsAbsolute and Position and call Analyze().</para>
    /// <para>2. Call AnalyzeS().</para>
    /// </summary>
    public unsafe class AnalyzeStrategyTree
    {
        #region Public API

        public class Stats
        {
            /// <summary>
            /// Sum of conditional probabilities of folds in nodes with a parent with a non-zero absolute strategic probability.
            /// </summary>
            public double SumNZaspFold
            {
                get;
                set;
            }

            /// <summary>
            /// Sum of conditional probabilities of calls in nodes with a parent with a non-zero absolute strategic probability.
            /// </summary>
            public double SumNZaspCall
            {
                get;
                set;
            }

            /// <summary>
            /// Sum of conditional probabilities of raises in nodes with a parent with a non-zero absolute strategic probability.
            /// </summary>
            public double SumNZaspRaise
            {
                get;
                set;
            }


            /// <summary>
            /// Numbers of nodes where the hero is acting with a non-zero absolute strategic probability.
            /// </summary>
            public Int64 NZaspMovesCount
            {
                get;
                set;
            }

        }

        public AnalyzeStrategyTree()
        {
            Output = Console.Out;
            IsAbsolute = true;
        }

        public StrategyTree StrategyTree
        {
            set;
            get;
        }

        /// <summary>
        /// If true, StrategyTree is treated as an absolute strategy, otherwise as a conditional strategy.
        /// Default: true.
        /// </summary>
        public bool IsAbsolute
        {
            set;
            get;
        }

        public int HeroPosition
        {
            set;
            get;
        }

        public bool IsVerbose
        {
            set;
            get;
        }

        /// <summary>
        /// Writes informational messages to this writer, default: Console.Out.
        /// </summary>
        public TextWriter Output
        {
            set;
            get;
        }

        public int MovesCount
        {
            protected set;
            get;
        }


        public int LeavesCount
        {
            protected set;
            get;
        }

        /// <summary>
        /// Total number of hero moves with zero abs. str. probability.
        /// Can be used to estimate "untoched" parts of the strategy tree. This is important for analyzing the strategy 
        /// of the hero. This is also important for the strategy of the opponent for such algos as FictitiousPlay.
        /// </summary>
        public UInt32 ZaspMovesCount
        {
            protected set;
            get;
        }

        /// <summary>
        /// Number of hero moves in leaves with zero abs. str. probability. See also ZaspMovesCount.
        /// </summary>
        public UInt32 ZaspLeavesCount
        {
            protected set;
            get;
        }

        /// <summary>
        /// For each round - statistics.
        /// </summary>
        public List<Stats> Statistics
        {
            get { return _stats; }
        }

        public void Analyze()
        {
            try
            {
                _playersCount = StrategyTree.PlayersCount;
                _stats = new List<Stats>(10);
                LeavesCount = 0;
                ZaspLeavesCount = 0;
                MovesCount = 0;
                ZaspMovesCount = 0;

                var walkTree = new WalkUFTreePP<StrategyTree, AnalyzeContext>();

                walkTree.OnNodeBegin = OnNodeBegin;
                walkTree.OnNodeEnd = OnNodeEnd;
                walkTree.Walk(StrategyTree);

                if (IsVerbose)
                {
                    Output.WriteLine("Analysis of strategy tree: '{0}'", StrategyTree.Version.Description);
                    Output.WriteLine("Players: {0}, nodes: {1:#,#}, leaves: {2:#,#}", StrategyTree.PlayersCount, StrategyTree.NodesCount, LeavesCount);
                    Output.Write("Blinds:");
                    for (int p = 0; p < _playersCount; ++p)
                    {
                        Output.Write(" {0}", StrategyTree.Nodes[p+1].Amount);
                    }
                    Output.WriteLine();

                    Output.WriteLine("Hero position: {0}, moves: {1:#,#}", HeroPosition, MovesCount);
                    Output.WriteLine("Zero str probab moves of hero: total {0} ({1:0.00%}), leaves: {2} ({3:0.00%})",
                        ZaspMovesCount, (double)ZaspMovesCount / MovesCount,
                        ZaspLeavesCount, (double)ZaspLeavesCount / LeavesCount);

                    Output.WriteLine("Action statistics in nodes with non-zero strategic probability:");
                    for(int r = 0; r < _stats.Count; ++r)
                    {
                        Console.WriteLine("Round {0}: nodes {1,10:#,#}, f: {2:0.00000}, c: {3:0.00000}, r: {4:0.00000}", r,
                            _stats[r].NZaspMovesCount,
                            _stats[r].SumNZaspFold / _stats[r].NZaspMovesCount,
                            _stats[r].SumNZaspCall / _stats[r].NZaspMovesCount,
                            _stats[r].SumNZaspRaise / _stats[r].NZaspMovesCount);
                    }
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Runs the analysis in verbose mode.
        /// </summary>
        public static void AnalyzeS(StrategyTree st, bool isAbsolute, int position)
        {
            AnalyzeStrategyTree analyzer = new AnalyzeStrategyTree { IsVerbose = true, StrategyTree = st, IsAbsolute = isAbsolute, HeroPosition = position };
            analyzer.Analyze();
        }

        #endregion

        #region Implementation

        private class AnalyzeContext : WalkUFTreePPContext
        {
            public double AbsStrProbab = 1;
            public StrategicState State;
            public int Round = -1;
            public bool IsHeroActingWithNonZeroProbab;
        }

        private void OnNodeBegin(StrategyTree tree, AnalyzeContext[] stack, int depth)
        {
            AnalyzeContext context = stack[depth];
            Int64 n = context.NodeIdx;
            if (depth == 0)
            {
                context.State = new StrategicState(_playersCount);
            }
            else
            {
                context.AbsStrProbab = stack[depth - 1].AbsStrProbab;
                context.Round = stack[depth - 1].Round;
                if (tree.Nodes[n].IsDealerAction)
                {
                    context.Round++;
                    context.State = stack[depth - 1].State;
                }
                else
                {
                    context.State = stack[depth - 1].State.GetNextState(tree.Nodes[n]);
                }
            }
            stack[depth].IsHeroActingWithNonZeroProbab = false;

            if (tree.Nodes[n].IsPlayerAction(HeroPosition) && context.Round >= 0)
            {
                if (tree.Nodes[n].Probab == 0)
                {
                    ZaspMovesCount++;
                }
                if (IsAbsolute)
                {
                    context.AbsStrProbab = tree.Nodes[n].Probab;
                }
                else
                {
                    context.AbsStrProbab *= tree.Nodes[n].Probab;
                }
                if (_stats.Count <= context.Round)
                {
                    _stats.Add(new Stats());
                }
                if (stack[depth - 1].AbsStrProbab > 0)
                {
                    stack[depth - 1].IsHeroActingWithNonZeroProbab = true;
                    double condStrProbab = context.AbsStrProbab/stack[depth - 1].AbsStrProbab;
                    if (context.State.HasStrictMaxInPot(HeroPosition))
                    {
                        _stats[context.Round].SumNZaspRaise += condStrProbab;
                    }
                    else if (tree.Nodes[n].Amount > 0 || context.State.HasMaxOrEqualInPot(HeroPosition))
                    {
                        _stats[context.Round].SumNZaspCall += condStrProbab;
                    }
                    else
                    {
                        _stats[context.Round].SumNZaspFold += condStrProbab;
                    }
                }
                MovesCount++;
            }
        }

        private void OnNodeEnd(StrategyTree tree, AnalyzeContext[] stack, int depth)
        {
            AnalyzeContext context = stack[depth];
            Int64 n = context.NodeIdx;
            if (context.ChildrenCount == 0)
            {
                LeavesCount++;
                if (tree.Nodes[n].IsPlayerAction(HeroPosition))
                {
                    if (tree.Nodes[n].Probab == 0)
                    {
                        ZaspLeavesCount++;
                    }
                }
            }
            if (context.IsHeroActingWithNonZeroProbab)
            {
                _stats[context.Round].NZaspMovesCount++;
            }
        }

        int _playersCount;
        List<Stats> _stats = new List<Stats>();

        #endregion
    }
}
