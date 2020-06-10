/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.lib.algorithms.tree;
using System.Diagnostics;
using ai.lib.algorithms.numbers;
using ai.pkr.metastrategy.vis;
using System.IO;
using System.Drawing;
using ai.lib.utils;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Calculates best response.
    /// Input: action and chance trees of the game, hero position, absolute strategy trees of the opponents (may use different abstractions).
    /// Output: hero best response strategy and game value.
    /// <para>This algo is designed to support relatively big games, but may fail on huge ones.</para>
    /// </summary>
    /// Here is pseudocode describing the ideas of the algorithm. The actual implementaion can deviate slightly.
    /// See also dev-doc/Br.pdn.
    /// 
    /// foreach heroLeaf in heroStrategyTree
    ///  heroLeafValue = 0
    ///  build heroStrategyHand from chance nodes in the heroStrategyTree leading to the heroLeaf
    ///  // Take chance nodes for the last player with the round heroLeaf.Round
    ///  foreach chanceNode in chanceTree
    ///    if heroHand != heroStrategyHand continue
    ///    
    ///    // Take all possible combination of oppLeaves with the same action path as of heroLeaf
    ///    foreach oppLeaves in oppStrategies
    ///      strategicProbab = 1
    ///      foreach oppLeaf in oppLeaves
    ///         strategicProbab *= oppLeaf.Probab
    ///      heroLeafValue += chanceNode.Probab * strategicProbab * (pot * potShare[HeroPosition] - inPot[HeroPosition])
    /// 
    /// // Now we have values in all nodes of the hero strategy tree and
    /// // can find the node values by the regular minimax algo and and then find the best response.
    /// CalculateNodeValue()
    /// FindBr()
    /// 
    /// To avoid too many searches in strategy trees, we will build some indexes similar to the ones in the GameValue.
    /// The structures built are described in the documentation.
    public unsafe class Br
    {
        #region Public classes

        public unsafe class Vis : VisStrategyTree<Vis.Context>
        {
            public class Context: VisStrategyTreeContext 
            {
                public double Value;
            }

            public Vis()
            {
                ShowExpr.Add(new ExprFormatter("s[d].Value", "\\nv:{1:0.00000}"));
            }


            public Br Solver
            {
                set;
                get;
            }

            public static void Show(Br solver, string fileName)
            {
                StrategyTree t = solver.Strategies[solver.HeroPosition];
                using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
                {
                    Vis vis = new Vis { Output = w, Solver = solver};
                    vis.Show(t);
                }
            }

            protected override void OnTreeBeginFunc(UFToUniAdapter aTree, int root)
            {
                StrategyTree tree = (StrategyTree)aTree.UfTree;
                base.OnTreeBeginFunc(aTree, root);
            }

            protected override bool OnNodeBeginFunc(UFToUniAdapter aTree, int n, List<Context> stack, int depth)
            {
                Context context = stack[depth];
                StrategyTree tree = (StrategyTree)aTree.UfTree;

                // Set default vis-fields
                SetContext(aTree, n, stack, depth);
                if (Solver._visGameValues != null)
                {
                    context.Value = Solver._visGameValues[n];
                }
                return base.OnNodeBeginFunc(aTree, n, stack, depth);
            }

            byte Gradient(byte col1, byte col2, double gr)
            {
                double result = col1 * gr + col2 * (1 - gr);
                return (byte)(Math.Min(255, result));
            }

            protected override void CustomizeNodeAttributes(UFToUniAdapter aTree, int n, List<Context> stack, int depth, Vis.NodeAttributeMap attr)
            {
                base.CustomizeNodeAttributes(aTree, n, stack, depth, attr);
                StrategyTree tree = (StrategyTree)aTree.UfTree;

                if (!tree.Nodes[n].IsDealerAction && tree.Nodes[n].Position == Solver.HeroPosition && tree.Nodes[n].Probab != 1.0)
                {
                    if (IsHtmlColor(attr.fillcolor))
                    {
                        attr.fillcolor = GradientHtmlColor(attr.fillcolor, Color.FromArgb(255, 200, 200, 200), 0.4);
                    }
                }
            }

            protected override void CustomizeEdgeAttributes(UFToUniAdapter aTree, int n, int pn, List<Context> stack, int depth, Vis.EdgeAttributeMap attr)
            {
                base.CustomizeEdgeAttributes(aTree, n, pn, stack, depth, attr);
                StrategyTree tree = (StrategyTree)aTree.UfTree;

                if (!tree.Nodes[n].IsDealerAction && tree.Nodes[n].Position == Solver.HeroPosition && tree.Nodes[n].Probab == 1.0)
                {
                    // Hero have chosen this edge - show thick
                    attr.penwidth = 3.0;
                }
            }

        }

        #endregion

        #region Public API

        public ActionTree ActionTree
        {
            set;
            get;
        }

        public ChanceTree ChanceTree
        {
            set;
            get;
        }

        /// <summary>
        /// Strategy trees of the opponent. Strategy tree of the hero is ignored. 
        /// After calling Solve the strategy tree of the hero will contain the Br strategy.
        /// </summary>
        public StrategyTree[] Strategies
        {
            set;
            get;
        }

        public int HeroPosition
        {
            set;
            get;
        }

        /// <summary>
        /// Game value of the hero.
        /// </summary> 
        public double Value
        {
            get;
            protected set;
        }

        /// <summary>
        /// If true, prepares game values data for visualization (memory consuming).
        /// Otherwise only best response strategy moves are shown.
        /// Default: false.
        /// </summary> 
        public bool PrepareVis
        {
            set;
            get;
        }

        public void Solve()
        {
            CheckPreconditions();
            Prepare();
            Calculate();           
            CleanUp();
        }

        #endregion

        #region Implementation

        class PrepareStrategicProbabsContext : WalkUFTreePPContext
        { 
            public Int64 ActionTreeIdx;
            public double Probab = 1;
            public int Round = -1;
            public int ChanceIdx = 0;
        }

        class PrepareChanceIndexContext : WalkUFTreePPContext
        {
            /// <summary>
            /// Indexes for each player.
            /// </summary>
            public int[] ChanceIdx = new int[10];
        }

        class CalculateValuesContext : WalkUFTreePPContext
        {
            public Int64 ActionTreeIdx;
            public int Round = -1;
            public int HeroChanceIdx = 0;
            public StrategicState State;
        }

        class CalculateBrContext: WalkUFTreePPContext
        {
            public bool IsBr = true;
        }

        private void CheckPreconditions()
        {
            if (ActionTree == null || ChanceTree == null || Strategies == null)
            {
                throw new ArgumentException("Input trees must not be null.");
            }
            if (ActionTree.PlayersCount != ChanceTree.PlayersCount || ActionTree.PlayersCount != Strategies.Length)
            {
                throw new ArgumentException("Inconsistent number of players in the input trees.");
            }
            if(HeroPosition < 0 || HeroPosition >= ActionTree.PlayersCount)
            {
                throw new ArgumentException("Hero position is out of range.");
            }
            for(int p = 0; p < Strategies.Length; ++p)
            {
                if (Strategies[p] == null && p != HeroPosition)
                {
                    throw new ArgumentException("Input trees must not be null.");
                }
            }
            if (ChanceTree.NodesCount > int.MaxValue)
            {
                throw new ArgumentException("Chance tree with size > int.MaxValue is not supported.");
            }
        }

        private void Prepare()
        {
            CreateHeroStrategy();

            _playersCount = Strategies.Length;
            _maxCard = new int[_playersCount][];
            _chanceIndexSizes = new int[_playersCount][];
            _strategicProbabs = new double[_playersCount][][].Fill(i => new double[ActionTree.NodesCount][]);
            if (PrepareVis)
            {
                _visGameValues = new double[Strategies[HeroPosition].NodesCount];
            }
            else
            {
                _visGameValues = null;
            }

            _roundsCount = ChanceTree.CalculateRoundsCount();
            for (int p = 0; p < _playersCount; ++p)
            {
                _maxCard[p] = new int[_roundsCount].Fill(int.MinValue);
                _chanceIndexSizes[p] = new int[_roundsCount].Fill(int.MinValue);
            }

            for (Int64 n = 1; n < ChanceTree.NodesCount; ++n)
            {
                int round = (ChanceTree.GetDepth(n) - 1) / _playersCount;
                if (_maxCard[ChanceTree.Nodes[n].Position][round] < ChanceTree.Nodes[n].Card)
                {
                    _maxCard[ChanceTree.Nodes[n].Position][round] = ChanceTree.Nodes[n].Card;
                }
            }

            for (int p = 0; p < _playersCount; ++p)
            {
                _chanceIndexSizes[p][0] = _maxCard[p][0] + 1;
                for (int r = 1; r < _roundsCount; ++r)
                {
                    _chanceIndexSizes[p][r] = _chanceIndexSizes[p][r - 1] * (_maxCard[p][r] + 1);
                }
            }

            _actionTreeIndex = new UFTreeChildrenIndex(ActionTree);

            for (Int64 n = 1; n < ActionTree.NodesCount; ++n)
            {
                if (_actionTreeIndex.GetChildrenCount(n) == 0)
                {
                    // This is a leaf
                    int round = ActionTree.Nodes[n].Round;
                    for (int p = 0; p < _playersCount; ++p)
                    {
                        if (p != HeroPosition)
                        {
                            _strategicProbabs[p][n] = new double[_chanceIndexSizes[p][round]];
                        }
                    }
                }
            }

            // Fill strategic probability for each player except the hero.
            for (_curPlayer = 0; _curPlayer < _playersCount; ++_curPlayer)
            {
                if (_curPlayer == HeroPosition)
                {
                    continue;
                }
                WalkUFTreePP<StrategyTree, PrepareStrategicProbabsContext> wt = new WalkUFTreePP<StrategyTree, PrepareStrategicProbabsContext>();
                wt.OnNodeBegin = PrepareStrategicProbabs_OnNodeBegin;
                wt.Walk(Strategies[_curPlayer]);
            }

            _chanceTreeNodes = new int[_roundsCount][][];

            for (int r = 0; r < _roundsCount; ++r)
            {
                int oppChanceIndexSize = 1;
                for(int p = 0; p < _playersCount; ++p)
                {
                    if(p == HeroPosition) continue;
                    oppChanceIndexSize *= _chanceIndexSizes[p][r];
                }
                int size = _chanceIndexSizes[HeroPosition][r];
                _chanceTreeNodes[r] = new int[size][];
                for (int i = 0; i < size; ++i)
                {
                    _chanceTreeNodes[r][i] = new int[oppChanceIndexSize];
                }
            }


            WalkUFTreePP<ChanceTree, PrepareChanceIndexContext> wt1 = new WalkUFTreePP<ChanceTree, PrepareChanceIndexContext>();
            wt1.OnNodeBegin = PrepareChanceIndex_OnNodeBegin;
            wt1.Walk(ChanceTree);
        }

        private void CreateHeroStrategy()
        {
            ChanceTree hct = ExtractPlayerChanceTree.ExtractS(ChanceTree, HeroPosition);
            Strategies[HeroPosition] = CreateStrategyTreeByChanceAndActionTrees.CreateS(hct, ActionTree);

            string description = String.Format("BR pos {0} vs ", HeroPosition);
            for (int p = 0; p < Strategies.Length; ++p)
            {
                if(p == HeroPosition) continue;
                description += String.Format("{0}: ({1}), ", p, Strategies[p].Version.Description);
            }
            description += String.Format("ct: ({0}), ", ChanceTree.Version.Description);
            description += String.Format("at: ({0})", ActionTree.Version.Description);

            Strategies[HeroPosition].Version.Description = description;
        }

        void PrepareStrategicProbabs_OnNodeBegin(StrategyTree tree, PrepareStrategicProbabsContext[] stack, int depth)
        {
            PrepareStrategicProbabsContext context = stack[depth];
            Int64 n = context.NodeIdx;

            if (depth == 0)
            {
                context.ActionTreeIdx = 0;
            }
            else
            {
                context.ActionTreeIdx = stack[depth - 1].ActionTreeIdx;
                context.Probab = stack[depth - 1].Probab;
                context.Round = stack[depth - 1].Round;
                context.ChanceIdx = stack[depth - 1].ChanceIdx;

                if (tree.Nodes[n].IsDealerAction)
                {
                    context.Round++;
                    context.ChanceIdx += CalculateChanceOffset(context.Round, _curPlayer) * tree.Nodes[n].Card;
                }
                else
                {
                    if (tree.Nodes[n].Position == _curPlayer)
                    {
                        context.Probab = n > _playersCount ? tree.Nodes[n].Probab : 1;
                    }
                    // Find the corresponding node in the action tree

                    context.ActionTreeIdx = FindActionTreeNodeIdx(tree, n, context.ActionTreeIdx, stack[depth-1].ChildrenCount - 1);

                    if (_actionTreeIndex.GetChildrenCount(context.ActionTreeIdx) == 0)
                    {
                        // A leaf.
                        _strategicProbabs[_curPlayer][context.ActionTreeIdx][context.ChanceIdx] = context.Probab;
                    }
                }
            }
        }

        void PrepareChanceIndex_OnNodeBegin(ChanceTree tree, PrepareChanceIndexContext[] stack, int depth)
        {
            PrepareChanceIndexContext context = stack[depth];
            Int64 n = context.NodeIdx;
            int round = (depth - 1) / _playersCount;

            if (depth > 0)
            {
                stack[depth - 1].ChanceIdx.CopyTo(context.ChanceIdx, 0);
                int curPlayer = tree.Nodes[n].Position;
                context.ChanceIdx[curPlayer] += CalculateChanceOffset(round, curPlayer)*tree.Nodes[n].Card;

                int oppChanceIdx = 0;
                int oppChanceIdxOffset = 1;
                for (int p = 0; p < _playersCount; ++p)
                {
                    if (p == HeroPosition) continue;
                    oppChanceIdx += oppChanceIdxOffset*context.ChanceIdx[p];
                    oppChanceIdxOffset *= _chanceIndexSizes[p][round];
                }

                if (curPlayer == _playersCount - 1)
                {
                    // All players got cards in this round - store the node index.
                    _chanceTreeNodes[round][context.ChanceIdx[HeroPosition]][oppChanceIdx] = (int)n;
                }
            }
        }


        int CalculateChanceOffset(int round, int player)
        {
            int offset = 1;
            for (int r = 0; r < round; ++r)
            {
                offset *= _maxCard[player][r] + 1;
            }
            return offset;
        }

        private long FindActionTreeNodeIdx(StrategyTree tree, long sNodeIdx, long aNodeIdx, int hintChildIdx)
        {
            long actionTreeIdx = ActionTree.FindChildByAmount(aNodeIdx,
                tree.Nodes[sNodeIdx].Amount,
                _actionTreeIndex, hintChildIdx); 
            if (actionTreeIdx == -1)
            {
                throw new ApplicationException(String.Format("Cannot find action tree node strategy node {0}", sNodeIdx));
            }
            return actionTreeIdx;
        }

        private void Calculate()
        {

            WalkUFTreePP<StrategyTree, CalculateValuesContext> wt = new WalkUFTreePP<StrategyTree, CalculateValuesContext>();
            wt.OnNodeBegin = CalculateValues_OnNodeBegin;
            wt.OnNodeEnd = CalculateValues_OnNodeEnd;
            wt.Walk(Strategies[HeroPosition]);

            Value = Strategies[HeroPosition].Nodes[0].Probab;

            WalkUFTreePP<StrategyTree, CalculateBrContext> wt1 = new WalkUFTreePP<StrategyTree, CalculateBrContext>();
            wt1.OnNodeBegin = CalculateBr_OnNodeBegin;
            wt1.OnNodeEnd = CalculateBr_OnNodeEnd;
            wt1.Walk(Strategies[HeroPosition]);
        }

        void CalculateValues_OnNodeBegin(StrategyTree tree, CalculateValuesContext[] stack, int depth)
        {
            CalculateValuesContext context = stack[depth];
            Int64 n = context.NodeIdx;

            // We'll use the probability to store game values temporarily.
            // Initialize with an unique value works both for summing and maximization.
            tree.Nodes[n].Probab = double.MinValue;

            if (depth == 0)
            {
                context.ActionTreeIdx = 0;
                context.State = new StrategicState(_playersCount);
            }
            else
            {
                context.ActionTreeIdx = stack[depth - 1].ActionTreeIdx;
                context.Round = stack[depth - 1].Round;
                context.HeroChanceIdx = stack[depth - 1].HeroChanceIdx;

                if (tree.Nodes[n].IsDealerAction)
                {
                    context.Round++;
                    context.HeroChanceIdx += CalculateChanceOffset(context.Round, HeroPosition) * tree.Nodes[n].Card;
                    context.State = stack[depth - 1].State;
                }
                else
                {
                    context.ActionTreeIdx = FindActionTreeNodeIdx(tree, n, context.ActionTreeIdx, stack[depth-1].ChildrenCount-1);
                    context.State = stack[depth - 1].State.GetNextState(ActionTree.Nodes[context.ActionTreeIdx]);
                }
            }
        }

        void CalculateValues_OnNodeEnd(StrategyTree tree, CalculateValuesContext[] stack, int depth)
        {
            CalculateValuesContext context = stack[depth];
            Int64 n = context.NodeIdx;

            if (_actionTreeIndex.GetChildrenCount(context.ActionTreeIdx) == 0)
            {
                // A leaf.
                int[] chanceNodes = _chanceTreeNodes[context.Round][context.HeroChanceIdx];
                double nodeValue = 0;

                EnumeratePlayersChanceIndex(context.Round, 0, context.ActionTreeIdx, chanceNodes,
                    0, 1, 1.0, context.State, ref nodeValue);

                tree.Nodes[n].Probab = nodeValue;
            }

            if (PrepareVis)
            {
                _visGameValues[n] = tree.Nodes[n].Probab;
            }

            if (n == 0)
            {
                return;
            }

            // Update parent.

            Int64 pn = stack[depth-1].NodeIdx;
            
            if (tree.Nodes[n].IsPlayerAction(HeroPosition))
            {
                tree.Nodes[pn].Probab = Math.Max(tree.Nodes[pn].Probab, tree.Nodes[n].Probab);
            }
            else
            {
                // Sum values
                if (tree.Nodes[pn].Probab == double.MinValue)
                {
                    tree.Nodes[pn].Probab = 0;
                }
                tree.Nodes[pn].Probab += tree.Nodes[n].Probab;
            }
        }


        void CalculateBr_OnNodeBegin(StrategyTree tree, CalculateBrContext[] stack, int depth)
        {
            CalculateBrContext context = stack[depth];
            Int64 n = context.NodeIdx;
            if (depth > 0)
            {
                context.IsBr = stack[depth - 1].IsBr;
            }

            if (tree.Nodes[n].IsPlayerAction(HeroPosition))
            {
                // Hero made a move
                Int64 pn = stack[depth - 1].NodeIdx;
                if(tree.Nodes[n].Probab == tree.Nodes[pn].Probab && context.IsBr)
                {
                    tree.Nodes[n].Probab = 1;
                    tree.Nodes[pn].Probab = double.MinValue;
                    context.IsBr = true;
                }
                else
                {
                    context.IsBr = false;
                    tree.Nodes[n].Probab = 0;
                }
            }
        }

        void CalculateBr_OnNodeEnd(StrategyTree tree, CalculateBrContext[] stack, int depth)
        {
            CalculateBrContext context = stack[depth];
            Int64 n = context.NodeIdx;

            if (tree.Nodes[n].IsDealerAction || tree.Nodes[n].Position != HeroPosition)
            {
                tree.Nodes[n].Probab = 0;
            }
        }


        void EnumeratePlayersChanceIndex(int round, int player, Int64 actionTreeNodeIdx, 
            int[] chanceNodes, int oppChanceIdx, int oppChanceIdxOffset,
            double oppStrategicProbab, StrategicState state, ref double nodeValue)
        {
            if (player == HeroPosition)
            {
                EnumeratePlayersChanceIndex(round, player + 1, actionTreeNodeIdx, 
                    chanceNodes, oppChanceIdx, oppChanceIdxOffset, 
                    oppStrategicProbab, state, ref nodeValue);
                return;
            }

            if (player == _playersCount)
            {
                int chanceNodeIdx = chanceNodes[oppChanceIdx];
                if (chanceNodeIdx == 0)
                {
                    // There is no existing cards for this index (a "hole")
                    return;
                }
                double[] potShares = new double[_playersCount];
                UInt16 activePlayers = ActionTree.Nodes[actionTreeNodeIdx].ActivePlayers;
                Debug.Assert(round == _roundsCount - 1 || CountBits.Count(activePlayers) == 1, "Must be either chance leaf or single active player");

                ChanceTree.Nodes[chanceNodeIdx].GetPotShare(activePlayers, potShares);
                double chanceProbab = ChanceTree.Nodes[chanceNodeIdx].Probab;
                double probab = chanceProbab * oppStrategicProbab;
                double heroValue = probab * (state.Pot * potShares[HeroPosition] - state.InPot[HeroPosition]);
                nodeValue += heroValue;
                return;
            }

            double[] strategicProbabs = _strategicProbabs[player][actionTreeNodeIdx];

            for (int ci = 0; ci < _chanceIndexSizes[player][round]; ++ci)
            {
                double strategicProbab = strategicProbabs[ci];
                EnumeratePlayersChanceIndex(round, player + 1, actionTreeNodeIdx, chanceNodes,
                    oppChanceIdx + oppChanceIdxOffset * ci, oppChanceIdxOffset * _chanceIndexSizes[player][round],
                    strategicProbab * oppStrategicProbab, state, ref nodeValue);
            }
        }

        private void CleanUp()
        {
            // Release large objects.
            _actionTreeIndex = null;
            _strategicProbabs = null;
            _chanceTreeNodes = null;
        }

        int _roundsCount;
        int[][] _maxCard;
        /// <summary>
        /// Numbers of chance indexes for each player and round.
        /// </summary>
        int[][] _chanceIndexSizes;
        /// <summary>
        /// Arrays with strategic probabilities.
        /// </summary>
        double[][][] _strategicProbabs;

        /// <summary>
        /// Index of the chance tree, for each round and each possible chance index of the hero contains 
        /// an array with node indexes of the chance tree. For each combination of opponents' cards
        /// there is an entry in this array. The entries that do not correspond to any possible cards are filled 
        /// with 0.
        /// </summary>
        int[][][] _chanceTreeNodes;

        /// <summary>
        /// Used for visualization only, contains node values for each node.
        /// </summary>
        double[] _visGameValues;

        int _playersCount;
        int _curPlayer;
        /// <summary>
        /// Children index of the action tree.
        /// </summary>
        UFTreeChildrenIndex _actionTreeIndex;

        #endregion
    }
}
