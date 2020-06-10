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
    /// Analyzes a chance tree. The results of comparison are stored in properites and can
    /// also be printed in verbose mode.
    /// </summary>
    public unsafe class AnalyzeChanceTree
    {
        #region Public API

        public AnalyzeChanceTree()
        {
            Output = Console.Out;
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

        /// <summary>
        /// If set, prints information about zero nodes to this file.
        /// </summary>
        public string ZeroNodesLogName
        {
            set;
            get;
        }

        public int LeavesCount
        {
            protected set;
            get;
        }

        /// <summary>
        /// Number of leaves with zero chance.
        /// </summary>
        public UInt32 ZeroChanceLeavesCount
        {
            protected set;
            get;
        }

        /// <summary>
        /// Number of leaves with non-zero chance but zero total pot share.
        /// </summary>
        public UInt32 ZeroPotSharesCount
        {
            protected set;
            get;
        }

        /// <summary>
        /// Sum of the pot shares in leaves for each position, 
        /// for zero-sum games expected to be LeavesCount/PlayersCount.
        /// </summary>
        public double [] SumPotShares
        {
            protected set;
            get;
        }

        public void Analyze(ChanceTree ct)
        {
            if (!string.IsNullOrEmpty(ZeroNodesLogName))
            {
                _zeroNodesLog = new StreamWriter(ZeroNodesLogName);
            }

            try
            {
                _playersCount = ct.PlayersCount;
                int roundsCount = ct.CalculateRoundsCount();
                _maxDepth =  roundsCount * _playersCount;
                _seenCards = new bool[roundsCount][][];
                for (int r = 0; r < roundsCount; ++r)
                {
                    _seenCards[r] = new bool[_playersCount][].Fill(i => new bool[0]);
                }
                LeavesCount = 0;
                SumPotShares = new double[_playersCount];
                ZeroChanceLeavesCount = ZeroPotSharesCount = 0;

                WalkUFTreePP<ChanceTree, AnalyzeContext> walkTree = new WalkUFTreePP<ChanceTree, AnalyzeContext>();

                walkTree.OnNodeBegin = OnNodeBegin;
                walkTree.Walk(ct);

                if (IsVerbose)
                {
                    Output.WriteLine("Analysis of chance tree: '{0}'", ct.Version.Description);
                    Output.WriteLine("Players: {0}, nodes: {1:#,#}, leaves: {2:#,#}", ct.PlayersCount, ct.NodesCount, LeavesCount);
                    Output.WriteLine("Leaves statistics:");
                    double totalPotShares = 0;
                    for (int p = 0; p < _playersCount; ++p)
                    {
                        totalPotShares += SumPotShares[p];
                        Output.WriteLine("Sum pot shares pos {0}: {1:0.000}", p, SumPotShares[p]);
                    }
                    Output.WriteLine("Sum pot shares total: {0:0.000}", totalPotShares);
                    Output.WriteLine("Zero leaves: chance: {0} ({1:0.00%}), pot shares: {2} ({3:0.00%})",
                                      ZeroChanceLeavesCount, (double) ZeroChanceLeavesCount/LeavesCount,
                                      ZeroPotSharesCount, (double) ZeroPotSharesCount/LeavesCount);

                    Output.WriteLine("Seen cards:");
                    for (int r = 0; r < roundsCount; ++r)
                    {
                        Output.WriteLine("Round {0}:", r);
                        for(int p = 0; p < _playersCount; ++p)
                        {
                            Output.Write("Pos {0}:", p);
                            int seenCount = 0;
                            for (int c = 0; c < _seenCards[r][p].Length; ++c)
                            {
                                if (_seenCards[r][p][c])
                                {
                                    seenCount++;
                                    Output.Write(" {0,2}", c);
                                }
                            }
                            Output.WriteLine(" ({0})", seenCount);
                        }
                    }
                }
            }
            finally
            {
                if(_zeroNodesLog != null)
                {
                    _zeroNodesLog.Close();
                    _zeroNodesLog = null;
                }
            }
        }

        /// <summary>
        /// Runs the analysis in verbose mode.
        /// </summary>
        public static void AnalyzeS(ChanceTree ct)
        {
            AnalyzeChanceTree analyzer = new AnalyzeChanceTree { IsVerbose = true };
            analyzer.Analyze(ct);
        }

        #endregion

        #region Implementation

        private class AnalyzeContext : WalkUFTreePPContext
        {
        }

        private void OnNodeBegin(ChanceTree tree, AnalyzeContext[] stack, int depth)
        {
            AnalyzeContext context = stack[depth];
            Int64 n = context.NodeIdx;
            int card = tree.Nodes[n].Card;

            if (depth > 0)
            {
                int round = (depth - 1)/_playersCount;
                int pos = tree.Nodes[n].Position;
                if (_seenCards[round][pos].Length <= card)
                {
                    Array.Resize(ref _seenCards[round][pos], card + 1);
                }
                _seenCards[round][pos][card] = true;
            }

            bool isLeaf = tree.GetDepth(n) == _maxDepth;

            if (!isLeaf)
            {
                return;
            }

            LeavesCount++;
            bool isZeroNode = false;

            double probab = tree.Nodes[n].Probab;
            if (probab == 0)
            {
                ZeroChanceLeavesCount++;
                isZeroNode = true;
            }

            UInt16[] activePlayers = ActivePlayers.Get(_playersCount, 2, _playersCount);

            double[] potShares = new double[_playersCount];
            double totalPotShare = 0;
            foreach (UInt16 ap in activePlayers)
            {
                tree.Nodes[n].GetPotShare(ap, potShares);
                for (int p = 0; p < _playersCount; ++p)
                {
                    SumPotShares[p] += potShares[p];
                    totalPotShare += potShares[p];
                }
            }

            if (probab >0 && totalPotShare == 0)
            {
                ZeroPotSharesCount++;
                isZeroNode = true;
            }

            if (isZeroNode && _zeroNodesLog != null)
            {
                _zeroNodesLog.Write("Node: {0,10} probab: {1:0.0000}, total pot share: {2:0.0000} ", 
                    n, probab, totalPotShare);
                for(int i = 1; i <= depth; ++i)
                {
                    _zeroNodesLog.Write("{0} ", tree.Nodes[stack[i].NodeIdx].ToStrategicString(null));
                }
                _zeroNodesLog.WriteLine();
            }
        }

        private TextWriter _zeroNodesLog;
        int _playersCount;
        int _maxDepth;
        // For each round and player: cards seen in the tree.
        bool[][][] _seenCards;


        #endregion
    }
}
