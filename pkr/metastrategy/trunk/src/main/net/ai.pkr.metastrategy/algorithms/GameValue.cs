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

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Calculates game value.
    /// <para>Input: action and chance trees of the game, absolute strategy trees of the players (may use different abstractions).</para>
    /// <para>Output: game value for each player.</para>
    /// <para>This algo is designed to support relatively big games, but may fail on huge ones.</para>
    /// </summary>
    /// Here is pseudocode describing the ideas of the algorithm. The actual implementaion can deviate slightly.
    /// See also dev-doc/GameValue.pdn.
    /// 
    /// foreach player
    ///   gameValue[player] = 0
    /// foreach actionLeaf in actionTree
    ///  // Take chance nodes for the last player with the round actionLeaf.Round
    ///  foreach chanceNode in chanceTree
    ///    strategicProbab = 1
    ///    foreach player
    ///      playerStrategicProbab = findStrategyNode(actionPath, chancePath)
    ///      strategicProbab *= playerStrategicProbab
    ///    foreach player   
    ///      gameValue[player] += chanceNode.Probab * strategicProbab * (pot * potShare[player] - inPot[player])
    ///      
    ///  To avoid too many searches in strategy trees, first we build some indexes:
    ///  1. For each player, for each round: max card index.
    ///  2. For each player, for each round: calculate length for array of probabilities:
    ///     len = 1
    ///     for each round
    ///       len*= (maxCard[round]+1)
    ///  3. For each leaf in action tree, for each player: allocate an array with the length calculated in step 2.
    ///  4. For each player: traverse strategy tree, in parallel traverse the action tree. For each leaf, 
    ///     store the strategic probability to an index:
    ///     i = 0
    ///     offset = 1
    ///     for round = 0 to curRound:
    ///       i += card[round] * offset
    ///       offset *= (maxCard[round]+1)
    ///       
    ///     This works best if the cards are in range [0, maxCard] without holes (this is usually so).
    ///     For example, if there are 2 cards in the round 0 and 3 cards in round 1, 
    ///     the hands go to the array like this:
    ///     Round 0: [0 1]
    ///     Round 1: [00 10 01 11 02 12]
    public unsafe class GameValue
    {
        #region Public classes

        public unsafe class Vis : VisStrategyTree<Vis.Context>
        {
            public class Context: VisStrategyTreeContext 
            {
                public Int64 ActionTreeIdx;
                public int ChanceIdx = 0;

                public double Value;
            }

            public Vis()
            {
                ShowExpr.Add(new ExprFormatter("s[d].Value", "\\nv:{1:0.00000}"));
            }


            public GameValue Solver
            {
                set;
                get;
            }

            public int Position
            {
                set;
                get;
            }

            public static void Show(GameValue solver, int position, string fileName)
            {
                StrategyTree t = solver.Strategies[position];
                using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
                {
                    Vis vis = new Vis { Output = w, Solver = solver, Position = position };
                    vis.Show(t);
                }
            }

            protected override bool OnNodeBeginFunc(UFToUniAdapter aTree, int n, List<Context> stack, int depth)
            {
                Context context = stack[depth];
                StrategyTree tree = (StrategyTree)aTree.UfTree;

                // Set default vis-fields
                SetContext(aTree, n, stack, depth);
                context.Value = 0;

                if (depth == 0)
                {
                    context.ActionTreeIdx = 0;
                }
                else
                {
                    context.ActionTreeIdx = stack[depth - 1].ActionTreeIdx;
                    context.ChanceIdx = stack[depth - 1].ChanceIdx;

                    if (tree.Nodes[n].IsDealerAction)
                    {
                        context.ChanceIdx += Solver.CalculateChanceOffset(context.Round, Position) * tree.Nodes[n].Card;
                    }
                    else
                    {
                        // Find the corresponding node in the action tree
                        context.ActionTreeIdx = Solver.FindActionTreeNodeIdx(tree, n, context.ActionTreeIdx,
                            stack[depth-1].ChildrenIterator - 1);

                        if (Solver._actionTreeIndex.GetChildrenCount(context.ActionTreeIdx) == 0)
                        {
                            // A leaf.
                            context.Value = Solver._visLeaveValues[Position][context.ActionTreeIdx][context.ChanceIdx];
                        }
                    }
                }

                return base.OnNodeBeginFunc(aTree, n, stack, depth);
            }

            protected override void OnNodeEndFunc(UFToUniAdapter aTree, int aNode, List<Vis.Context> stack, int depth)
            {
                Context context = stack[depth];
                StrategyTree tree = (StrategyTree)aTree.UfTree;

                if (depth > 0)
                {
                    stack[depth - 1].Value += context.Value;
                }

                base.OnNodeEndFunc(aTree, aNode, stack, depth);
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

        public StrategyTree[] Strategies
        {
            set;
            get;
        }

        /// <summary>
        /// Game value for each player.
        /// </summary>
        public double[] Values
        {
            get 
            {
                return _gameValues;
            }
        }

        /// <summary>
        /// If true, prepares data for visualization (memory-consuming).
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

        class PrepareContext : WalkUFTreePPContext
        { 
            public Int64 ActionTreeIdx;
            public double Probab = 1;
            public int Round = -1;
            public int ChanceIdx = 0;
        }

        class CalculateActionContext : WalkUFTreePPContext
        {
            public StrategicState State;
        }

        class CalculateChanceContext : WalkUFTreePPContext
        {
            public int Round = -1;
            /// <summary>
            /// Indexes for each player.
            /// </summary>
            public int[] ChanceIdx = new int[10];
            public double StrategicProbab = 1.0;
        }

        private void CheckPreconditions()
        {
            if (ActionTree == null || ChanceTree == null || Strategies == null)
            {
                throw new ArgumentException("Input trees must not be null.");
            }
            foreach (StrategyTree st in Strategies)
            {
                if (st == null)
                {
                    throw new ArgumentException("Input trees must not be null.");
                }
            }
            if (ActionTree.Nodes[0].Position != ChanceTree.Nodes[0].Position || ActionTree.Nodes[0].Position != Strategies.Length)
            {
                throw new ArgumentException("Inconsistent number of players in the input trees.");
            }
        }


        private void Prepare()
        {
            _playersCount = Strategies.Length;
            _maxCard = new int[_playersCount][];
            _spArrayLengths = new int[_playersCount][];
            _spArrays = new double[_playersCount][][].Fill(i => new double[ActionTree.NodesCount][]);
            if (PrepareVis)
            {
                _visLeaveValues = new double[_playersCount][][].Fill(i => new double[ActionTree.NodesCount][]);
            }
            _gameValues = new double[_playersCount];

            _roundsCount = ChanceTree.CalculateRoundsCount();
            for (int p = 0; p < _playersCount; ++p)
            {
                _maxCard[p] = new int[_roundsCount].Fill(int.MinValue);
                _spArrayLengths[p] = new int[_roundsCount].Fill(int.MinValue);
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
                _spArrayLengths[p][0] = _maxCard[p][0] + 1;
                for (int r = 1; r < _roundsCount; ++r)
                {
                    _spArrayLengths[p][r] = _spArrayLengths[p][r - 1] * (_maxCard[p][r] + 1);
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

                        _spArrays[p][n] = new double[_spArrayLengths[p][round]];
                        if (PrepareVis)
                        {
                            _visLeaveValues[p][n] = new double[_spArrayLengths[p][round]];
                        }
                    }
                }
            }

            for (_curPlayer = 0; _curPlayer < _playersCount; ++_curPlayer)
            {
                WalkUFTreePP<StrategyTree, PrepareContext> wt = new WalkUFTreePP<StrategyTree, PrepareContext>();
                wt.OnNodeBegin = Prepare_OnNodeBegin;
                wt.Walk(Strategies[_curPlayer]);
            }
        }

        void Prepare_OnNodeBegin(StrategyTree tree, PrepareContext[] stack, int depth)
        {
            PrepareContext context = stack[depth];
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

                    context.ActionTreeIdx = FindActionTreeNodeIdx(tree, n, context.ActionTreeIdx, stack[depth-1].ChildrenCount-1);

                    if (_actionTreeIndex.GetChildrenCount(context.ActionTreeIdx) == 0)
                    {
                        // A leaf.
                        _spArrays[_curPlayer][context.ActionTreeIdx][context.ChanceIdx] = context.Probab;
                    }
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
                throw new ApplicationException(
                    String.Format("Cannot find action tree node for player {0}, strategy node {1}", _curPlayer, sNodeIdx));
            }
            return actionTreeIdx;
        }

        private void Calculate()
        {

            WalkUFTreePP<ActionTree, CalculateActionContext> wt = new WalkUFTreePP<ActionTree, CalculateActionContext>();
            wt.OnNodeBegin = Calculate_Action_OnNodeBegin;
            wt.Walk(ActionTree);
        }

        void Calculate_Action_OnNodeBegin(ActionTree tree, CalculateActionContext[] stack, int depth)
        {
            CalculateActionContext context = stack[depth];
            Int64 n = context.NodeIdx;

            if (depth == 0)
            {
                context.State = new StrategicState(_playersCount);
            }
            else
            {
                context.State = stack[depth - 1].State.GetNextState(tree.Nodes[n]);
            }

            if (_actionTreeIndex.GetChildrenCount(n) != 0)
            {
                return;
            }
            // This is a leaf
            int round = ActionTree.Nodes[n].Round;
            _chanceDepth = (byte)(_playersCount * (round+1));
            _strategicState = context.State;
            _actionTreeNodeIdx = n;
            WalkUFTreePP<ChanceTree, CalculateChanceContext> wt = new WalkUFTreePP<ChanceTree, CalculateChanceContext>();
            wt.OnNodeBegin = Calculate_Chance_OnNodeBegin;
            wt.Walk(ChanceTree);
        }

        void Calculate_Chance_OnNodeBegin(ChanceTree tree, CalculateChanceContext[] stack, int depth)
        {
            CalculateChanceContext context = stack[depth];
            Int64 n = context.NodeIdx;
            // We are interested in nodes up to chanceDepth.
            // Now we are traversing the whole tree in each round, but ignore the nodes of rounds 
            // greater than the round of the current action leave. This can be optimized by indexing if necessary.
            // But this probably will not bring much because the most of work is done in the last round:
            // there are more action nodes and the whole chance tree must be traversed.
            if (depth > _chanceDepth)
            {
                return;
            }

            if (depth > 0)
            {
                context.Round = stack[depth - 1].Round;
                context.StrategicProbab = stack[depth - 1].StrategicProbab;
                stack[depth - 1].ChanceIdx.CopyTo(context.ChanceIdx, 0);
                if (tree.Nodes[n].Position == 0)
                {
                    context.Round++;
                }
                int curPlayer = tree.Nodes[n].Position;
                context.ChanceIdx[curPlayer] += CalculateChanceOffset(context.Round, curPlayer) * tree.Nodes[n].Card;

                int player = _playersCount - _chanceDepth + depth - 1;

                if (player >= 0)
                {
                    double[] probabArray = _spArrays[player][_actionTreeNodeIdx];
                    double playerStrategicProbab = probabArray[context.ChanceIdx[player]];
                    context.StrategicProbab *= playerStrategicProbab;

                    if (depth == _chanceDepth)
                    {
                        double[] potShares = new double[_playersCount];
                        UInt16 activePlayers = ActionTree.Nodes[_actionTreeNodeIdx].ActivePlayers;
                        Debug.Assert(context.Round == _roundsCount - 1 || CountBits.Count(activePlayers) == 1,
                                     "Must be either chance leaf or single active player");
                        ChanceTree.Nodes[n].GetPotShare(activePlayers, potShares);
                        double chanceProbab = ChanceTree.Nodes[n].Probab;
                        double probab = chanceProbab * context.StrategicProbab;
                        double pot = _strategicState.Pot;
                        for (int p = 0; p < _playersCount; ++p)
                        {
                            double playerValue =  probab * (pot*potShares[p] - _strategicState.InPot[p]);
                            _gameValues[p] += playerValue;
                            if (PrepareVis)
                            {
                                _visLeaveValues[p][_actionTreeNodeIdx][context.ChanceIdx[p]] += playerValue;
                            }
                        }
                    }
                }
            }
        }


        private void CleanUp()
        {
            // Release large objects.
            if (!PrepareVis)
            {
                _actionTreeIndex = null;
            }
            _spArrays = null;
        }

        int _roundsCount;
        int[][] _maxCard;
        /// <summary>
        /// Length of arrays with strategic probabilites.
        /// </summary>
        int[][] _spArrayLengths;
        /// <summary>
        /// Arrays with strategic probabilities.
        /// </summary>
        double[][][] _spArrays;
        double[][][] _visLeaveValues;
        double[] _gameValues;
        int _playersCount;
        int _curPlayer;
        UFTreeChildrenIndex _actionTreeIndex;
        Int64 _actionTreeNodeIdx;
        byte _chanceDepth;
        StrategicState _strategicState;
        #endregion
    }
}
