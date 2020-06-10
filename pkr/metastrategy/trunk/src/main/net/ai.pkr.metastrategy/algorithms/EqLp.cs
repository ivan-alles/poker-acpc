/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

#if DEBUG
//#define DEBUG_LPSOLVE
#endif
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
using System.Globalization;
using lpsolve55;
using ai.lib.utils;

#if DEBUG_LPSOLVE
#warning Compiling with debug information or features impacting performance and memory requirements.
#endif

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Finds equilibrium strategy in a 2-players game by solving a linear program.
    /// Input: action and chance trees of the game, hero position. Players may use different abstractions.
    /// Output:  equilibrium strategy and game value for the hero.
    /// </summary>
    /// Developer notes:
    /// The algo used to build the LP is similar to Br algo, but the hero and the opponents are reversed.
    public unsafe class EqLp
    {
        #region Public API

        public EqLp()
        {
            LpsolveOutFile = "";
        }

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
        /// Eq-strategy of the hero.
        /// </summary>
        public StrategyTree Strategy
        {
            protected set;
            get;
        }

        public bool IsLpSolutionFound
        {
            get;
            private set;
        }

        /// <summary>
        /// File name for lpsolve to print debug information.
        /// If null, is redirected to console.
        /// If "", output is ignored (default).
        /// Otherwise LP solve will create this file.
        /// </summary>
        public string LpsolveOutFile
        {
            set;
            get;
        }

        public void Solve()
        {
            CheckPreconditions();
            Prepare();
            SolveLp();
            CopySolutionToStrategy();
            CleanUp();
        }

        /// <summary>
        /// Solves a game for one position. If solution is found, returns it, otherwise returns null.
        /// </summary>
        public static StrategyTree Solve(ActionTree at, ChanceTree ct, int heroPos, out double value)
        {
            EqLp solver = new EqLp { ChanceTree = ct, ActionTree = at, HeroPosition = heroPos };
            solver.Solve();
            value = solver.Value;
            if (!solver.IsLpSolutionFound)
            {
                return null;
            }
            return solver.Strategy;
        }

        /// <summary>
        /// Solves a game for all positions. If solution for all positions is found, returns it, otherwise returns null.
        /// </summary>
        public static StrategyTree[] Solve(ActionTree at, ChanceTree ct, out double[] values)
        {
            StrategyTree [] strategies = new StrategyTree[2];
            values = new double[2];
            for (int heroPos = 0; heroPos < 2; ++heroPos)
            {
                strategies[heroPos] = Solve(at, ct, heroPos, out values[heroPos]);
                if (strategies[heroPos] == null)
                {
                    return null;
                }
            }
            return strategies;
        }

        /// <summary>
        /// Prints the problem in CPLEX format.
        /// </summary>
        public void PrintCPLEX(TextWriter wr)
        {
            wr.WriteLine("\\\\Vars: {0}, constr le: {1}, constr eq: {2}",
                         _variables.Count, _constraintsLE.Count, _constraintsEQ.Count);
            wr.Write("\\\\Vars:");
            for (int i = 0; i < _variables.Count; ++i)
            {
                wr.Write(_variables.GetName(i));
                wr.Write(" ");
            }
            wr.WriteLine();
            wr.WriteLine("Maximize");
            wr.WriteLine("v0");

            wr.WriteLine("Subject To");
            wr.WriteLine("\\\\Constraints less than or equal:");
            for (int i = 0; i < _constraintsLE.Count; ++i)
            {
                wr.Write("c_le{0}:  ", i);
                wr.WriteLine(_constraintsLE[i].ToString(_variables, " <= "));
            }
            wr.WriteLine();
            wr.WriteLine("\\\\Constraints equal:");
            for (int i = 0; i < _constraintsEQ.Count; ++i)
            {
                wr.Write("c_eq{0}:  ", i);
                wr.WriteLine(_constraintsEQ[i].ToString(_variables, "  = "));
            }
            wr.WriteLine();
            wr.WriteLine("Bounds");
            for (int i = 0; i < _variables.Count; ++i)
            {
                string varName = _variables.GetName(i);
                if (varName.StartsWith("h"))
                {
                    wr.WriteLine("0 <= {0} < +inf", varName);
                }
                else
                {
                    wr.WriteLine("-inf <= {0} < +inf", varName);
                }
            }
            wr.WriteLine("End");
        }


        #endregion

        #region Implementation

        /// <summary>
        /// LP variables.
        /// </summary>
        protected class Variables
        {
            public int Add(string type, int nodeId)
            {
                string name = type + nodeId.ToString();
                int ordinal = _list.Count;
                _list.Add(name);
                return ordinal;
            }

            public string GetName(int index)
            {
                return _list[index];
            }

            public int Count
            {
                get { return _list.Count; }
            }

            private List<string> _list = new List<string>();
        }

        /// <summary>
        /// LP constaint.
        /// </summary>
        protected class Constraint
        {
            public List<double> Vector
            {
                get
                {
                    return _vector;
                }
            }

            public void AddVariable(int index, double coeff)
            {
                while (_vector.Count <= index)
                {
                    _vector.Add(0);
                }
                _vector[index] = _vector[index] + coeff;
            }

            public double RightHandSide
            {
                set;
                get;
            }

            public string ToString(Variables vars, string type)
            {
                // string floatFmt = ":0.000";
                string floatFmt = "";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _vector.Count; ++i)
                {
                    if (_vector[i] == 0)
                        continue;
                    sb.AppendFormat(" {0} ", Sign(_vector[i]));
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0" + floatFmt + "} {1}", Math.Abs(_vector[i]), vars.GetName(i));
                }
                sb.Append(type);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0" + floatFmt + "}", RightHandSide);
                return sb.ToString();
            }

            private string Sign(double value)
            {
                return value >= 0 ? "+" : "-";
            }

            private List<double> _vector = new List<double>();
        }


        class PrepareHeroVarsContext : WalkUFTreePPContext
        { 
            public Int64 ActionTreeIdx;
            public int Round = -1;
            public int ChanceIdx = 0;
            public int VarH = -1;
            public Constraint EqConstr;
        }

        class PrepareChanceIndexContext : WalkUFTreePPContext
        {
            /// <summary>
            /// Indexes for each player.
            /// </summary>
            public int[] ChanceIdx = new int[2];
        }

        class PrepareOppContext : WalkUFTreePPContext
        {
            public Int64 ActionTreeIdx;
            public int Round = -1;
            public int OppChanceIdx = 0;
            public StrategicState State;
            public Constraint EqConstr;
            public int VarV;
            /// <summary>
            /// r-variables of children (used for nodes where the opponent is acting).
            /// </summary>
            public List<int> ChildVarsR = new List<int>();
            /// <summary>
            /// v-variables of children (used for nodes where the opponent is acting).
            /// </summary>
            public List<int> ChildVarsV = new List<int>();
        }

        class CopySolutionToStrategyContext : WalkUFTreePPContext
        {
        }

        private void CheckPreconditions()
        {
            if (ActionTree == null || ChanceTree == null)
            {
                throw new ArgumentException("Input trees must not be null.");
            }
            if (ActionTree.PlayersCount != 2 || ActionTree.PlayersCount != 2)
            {
                throw new ArgumentException("Only heads-up games are supporterd.");
            }
            if(HeroPosition < 0 || HeroPosition >= ActionTree.PlayersCount)
            {
                throw new ArgumentException("Hero position is out of range.");
            }
            if (ChanceTree.NodesCount > int.MaxValue)
            {
                throw new ArgumentException("Chance tree with size > int.MaxValue is not supported.");
            }
        }


        void Prepare()
        {
            _oppPosition = 1 - HeroPosition;
            
            // Create a strategy for each player. We need both because players can use different abstractions.
            PrepareHeroStrategy();
            ChanceTree pct;

            pct = ExtractPlayerChanceTree.ExtractS(ChanceTree, _oppPosition);
            _oppStrategy = CreateStrategyTreeByChanceAndActionTrees.CreateS(pct, ActionTree);

            _variables = new Variables();
            _constraintsLE = new List<Constraint>();
            _constraintsEQ = new List<Constraint>();

            PrepareHero();

            // Create index for the chance tree.
            _chanceTreeNodes = new int[PLAYERS_COUNT][][];
            for (int r = 0; r < _roundsCount; ++r)
            {
                int oppSize = _chanceIndexSizes[_oppPosition][r];
                _chanceTreeNodes[r] = new int[oppSize][];
                int heroSize = _chanceIndexSizes[HeroPosition][r];
                for (int i = 0; i < oppSize; ++i)
                {
                    _chanceTreeNodes[r][i] = new int[heroSize];
                }
            }

            WalkUFTreePP<ChanceTree, PrepareChanceIndexContext> wt1 = new WalkUFTreePP<ChanceTree, PrepareChanceIndexContext>();
            wt1.OnNodeBegin = PrepareChanceIndex_OnNodeBegin;
            wt1.Walk(ChanceTree);

            // This will be the index of v0 variable, because it will be added next.
            _v0 = _variables.Count;

            PrepareOpp();
        }

        private void PrepareHeroStrategy()
        {
            ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ChanceTree, HeroPosition);
            Strategy = CreateStrategyTreeByChanceAndActionTrees.CreateS(pct, ActionTree);

            string description = String.Format("EQ (LP) pos {0} ", HeroPosition);
            description += String.Format("ct: ({0}), ", ChanceTree.Version.Description);
            description += String.Format("at: ({0})", ActionTree.Version.Description);

            Strategy.Version.Description = description;
        }

        private void PrepareHero()
        {
            _maxCard = new int[PLAYERS_COUNT][];
            _chanceIndexSizes = new int[PLAYERS_COUNT][];
            _heroVarsInLeaves = new int[ActionTree.NodesCount][];

            _roundsCount = ChanceTree.CalculateRoundsCount();
            for (int p = 0; p < PLAYERS_COUNT; ++p)
            {
                _maxCard[p] = new int[_roundsCount].Fill(int.MinValue);
                _chanceIndexSizes[p] = new int[_roundsCount].Fill(int.MinValue);
            }

            for (Int64 n = 1; n < ChanceTree.NodesCount; ++n)
            {
                int round = (ChanceTree.GetDepth(n) - 1) / PLAYERS_COUNT;
                if (_maxCard[ChanceTree.Nodes[n].Position][round] < ChanceTree.Nodes[n].Card)
                {
                    _maxCard[ChanceTree.Nodes[n].Position][round] = ChanceTree.Nodes[n].Card;
                }
            }

            for (int p = 0; p < PLAYERS_COUNT; ++p)
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
                    _heroVarsInLeaves[n] = new int[_chanceIndexSizes[HeroPosition][round]].Fill(-1);
                }
            }

            // Create hero variables
            WalkUFTreePP<StrategyTree, PrepareHeroVarsContext> wt = new WalkUFTreePP<StrategyTree, PrepareHeroVarsContext>();
            wt.OnNodeBegin = PrepareHero_OnNodeBegin;
            wt.Walk(Strategy, PLAYERS_COUNT);
        }

        void PrepareHero_OnNodeBegin(StrategyTree tree, PrepareHeroVarsContext[] stack, int depth)
        {
            PrepareHeroVarsContext context = stack[depth];
            Int64 n = context.NodeIdx;

            if (depth == PLAYERS_COUNT)
            {
                // Skip blinds
                context.ActionTreeIdx = PLAYERS_COUNT;
            }
            else
            {
                context.ActionTreeIdx = stack[depth - 1].ActionTreeIdx;
                context.Round = stack[depth - 1].Round;
                context.ChanceIdx = stack[depth - 1].ChanceIdx;
                context.VarH = stack[depth - 1].VarH;
                context.EqConstr = null;

                if (tree.Nodes[n].IsDealerAction)
                {
                    context.Round++;
                    context.ChanceIdx += CalculateChanceOffset(context.Round, HeroPosition) * tree.Nodes[n].Card;
                }
                else
                {
                    context.ActionTreeIdx = FindActionTreeNodeIdx(tree, n, context.ActionTreeIdx, stack[depth-1].ChildrenCount-1);
                    if (tree.Nodes[n].IsPlayerAction(HeroPosition))
                    {
                        Constraint constr = stack[depth - 1].EqConstr;
                        if (stack[depth - 1].EqConstr == null)
                        {
                            // Create new constraint
                            constr = new Constraint();
                            stack[depth - 1].EqConstr = constr;
                            _constraintsEQ.Add(constr);
                            if (context.VarH == -1)
                            {
                                // This is the very first move of the hero in this tree branch,
                                // add constraint Sum(h_child) = 1 
                                constr.RightHandSide = 1;
                            }
                            else
                            {
                                // Add constraint h_prev_move = Sum(h_child)
                                constr.RightHandSide = 0;
                                constr.AddVariable(stack[depth - 1].VarH, -1);
                            }
                        }
                        context.VarH = _variables.Add("h", (int)n);
                        constr.AddVariable(context.VarH, 1);
                    }

                    if (_actionTreeIndex.GetChildrenCount(context.ActionTreeIdx) == 0)
                    {
                        // A leaf.
                        _heroVarsInLeaves[context.ActionTreeIdx][context.ChanceIdx] = context.VarH;
                    }
                }
            }
        }

        void PrepareChanceIndex_OnNodeBegin(ChanceTree tree, PrepareChanceIndexContext[] stack, int depth)
        {
            PrepareChanceIndexContext context = stack[depth];
            Int64 n = context.NodeIdx;
            int round = (depth - 1) / PLAYERS_COUNT;

            if (depth > 0)
            {
                stack[depth - 1].ChanceIdx.CopyTo(context.ChanceIdx, 0);
                int curPlayer = tree.Nodes[n].Position;
                context.ChanceIdx[curPlayer] += CalculateChanceOffset(round, curPlayer)*tree.Nodes[n].Card;

                if (curPlayer == PLAYERS_COUNT - 1)
                {
                    // All players got cards in this round - store the node index.
                    _chanceTreeNodes[round][context.ChanceIdx[_oppPosition]][context.ChanceIdx[HeroPosition]] = (int)n;
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
                tree.Nodes[sNodeIdx].Amount, _actionTreeIndex, hintChildIdx);
            if (actionTreeIdx == -1)
            {
                throw new ApplicationException(String.Format("Cannot find action tree node, strategy node {0}", sNodeIdx));
            }
            return actionTreeIdx;
        }

        private void PrepareOpp()
        {
            WalkUFTreePP<StrategyTree, PrepareOppContext> wt = new WalkUFTreePP<StrategyTree, PrepareOppContext>();
            wt.OnNodeBegin = PrepareOpp_OnNodeBegin;
            wt.OnNodeEnd = PrepareOpp_OnNodeEnd;
            wt.Walk(_oppStrategy, PLAYERS_COUNT);
        }

        void PrepareOpp_OnNodeBegin(StrategyTree tree, PrepareOppContext[] stack, int depth)
        {
            PrepareOppContext context = stack[depth];
            Int64 n = context.NodeIdx;

            // Add v-var here once for all cases.
            context.VarV = _variables.Add("v", (int)n);
            // Initialize fields for post-oreder procecessing, etc.
            context.EqConstr = null;
            context.ChildVarsR.Clear();
            context.ChildVarsV.Clear();
            //context.LeConstr = null;

            if (depth == PLAYERS_COUNT)
            {
                // Skip blinds
                context.ActionTreeIdx = PLAYERS_COUNT;
                context.State = new StrategicState(PLAYERS_COUNT);
                for (int a = 1; a <= PLAYERS_COUNT; ++a)
                {
                    context.State = context.State.GetNextState(ActionTree.Nodes[a]);
                }
            }
            else
            {
                context.ActionTreeIdx = stack[depth - 1].ActionTreeIdx;
                context.Round = stack[depth - 1].Round;
                context.OppChanceIdx = stack[depth - 1].OppChanceIdx;

                if (tree.Nodes[n].IsDealerAction)
                {
                    context.Round++;
                    context.OppChanceIdx += CalculateChanceOffset(context.Round, _oppPosition) * tree.Nodes[n].Card;
                    context.State = stack[depth - 1].State;
                }
                else
                {
                    context.ActionTreeIdx = FindActionTreeNodeIdx(tree, n, context.ActionTreeIdx, stack[depth-1].ChildrenCount-1);
                    context.State = stack[depth - 1].State.GetNextState(ActionTree.Nodes[context.ActionTreeIdx]);
                }
            }
        }

        void PrepareOpp_OnNodeEnd(StrategyTree tree, PrepareOppContext[] stack, int depth)
        {
            PrepareOppContext context = stack[depth];
            Int64 n = context.NodeIdx;

            if (_actionTreeIndex.GetChildrenCount(context.ActionTreeIdx) == 0)
            {
                // A leaf, add new variables and constraints:
                // v6 = k16 * h16 + k26 * h26
                Constraint constr = new Constraint();
                constr.RightHandSide = 0;
                constr.AddVariable(context.VarV, -1);
                _constraintsEQ.Add(constr);
                int[] chanceNodes = _chanceTreeNodes[context.Round][context.OppChanceIdx];
                EnumeratePlayersChanceIndex(context.ActionTreeIdx, chanceNodes, context.State, constr);
            }

            if (n == PLAYERS_COUNT)
            {
                return;
            }

            // Update parent.

            Int64 pn = stack[depth - 1].NodeIdx;
            PrepareOppContext parentContext = stack[depth - 1];

            Constraint constrEq = parentContext.EqConstr;
            if (constrEq == null)
            {
                // Create new eq constraint in the parent.
                constrEq = new Constraint();
                parentContext.EqConstr = constrEq;
                constrEq.RightHandSide = 0;
                constrEq.AddVariable(parentContext.VarV, -1);
                _constraintsEQ.Add(constrEq);
            }

            if (tree.Nodes[n].IsPlayerAction(_oppPosition))
            {
                // v2 = r3 + r8
                // Add r-variable for each node where opponent have acted.
                int varR = _variables.Add("r", (int) n);
                constrEq.AddVariable(varR, 1);
                parentContext.ChildVarsR.Add(varR);
                parentContext.ChildVarsV.Add(context.VarV);
            }
            else
            {
                Debug.Assert(context.ChildVarsV.Count == context.ChildVarsR.Count);
                // If it is the node where the opponent is acting, 
                // add le-constraints.
                // r3 + r8 <= v3
                // r3 + r8 <= v8
                foreach (int varV in context.ChildVarsV)
                {
                    Constraint constrLe = new Constraint();
                    _constraintsLE.Add(constrLe);
                    constrLe.RightHandSide = 0;
                    constrLe.AddVariable(varV, -1);
                    foreach (int varR in context.ChildVarsR)
                    {
                        constrLe.AddVariable(varR, 1);
                    }
                }

                // Sum all the children in the parent:
                // v3 = v4 + v5
                constrEq.AddVariable(context.VarV, 1);
            }
        }

        void EnumeratePlayersChanceIndex(Int64 actionTreeNodeIdx, int[] chanceNodes, StrategicState state, Constraint constr)
        {
            for (int i = 0; i < _heroVarsInLeaves[actionTreeNodeIdx].Length; ++i)
            {
                int chanceNodeIdx = chanceNodes[i];
                if (chanceNodeIdx == 0)
                {
                    // There is no existing cards for this index (a "hole")
                    continue;
                }
                int varH = _heroVarsInLeaves[actionTreeNodeIdx][i];

                double[] potShares = new double[PLAYERS_COUNT];
                UInt16 activePlayers = ActionTree.Nodes[actionTreeNodeIdx].ActivePlayers;
                //Debug.Assert(round == _roundsCount - 1 || CountBits.Count(activePlayers) == 1, "Must be either chance leaf or single active player");

                ChanceTree.Nodes[chanceNodeIdx].GetPotShare(activePlayers, potShares);
                double chanceProbab = ChanceTree.Nodes[chanceNodeIdx].Probab;
                double result = state.Pot * potShares[HeroPosition] - state.InPot[HeroPosition];
                double coeff = chanceProbab * result;
                if (varH != -1)
                {

                    constr.AddVariable(varH, coeff);
                }
                else
                {
                    // Special case - opponent folded before the hero has acted (e.g. small blind folds at the very beginning).
                    // In this case there is no hero variable, and the v-variable is constant.
                    // Constaint has the form: v3 = k3 + k13, or -v3 = -k3 - k13
                    constr.RightHandSide -= coeff;
                }
           }
        }

        void SolveLp()
        {
            string runtime = "win32";
            if (System.IntPtr.Size == 8) runtime = "win64";
            string pathToDll = Props.Global.Expand("${bds.BinDir}")+runtime;
            bool isInitOk = lpsolve.Init(pathToDll);
            if (!isInitOk)
            {
                throw new ApplicationException("Cannot initialize lpsolve");
            }

            const string NewLine = "\n";

            int lp;
            int release = 0, Major = 0, Minor = 0, build = 0;
            double[] row;

            lp = lpsolve.make_lp(_constraintsLE.Count + _constraintsEQ.Count, _variables.Count);

            lpsolve.lp_solve_version(ref Major, ref Minor, ref release, ref build);

#if DEBUG_LPSOLVE 
            /* let's first demonstrate the logfunc callback feature */
            lpsolve.put_logfunc(lp, new lpsolve.logfunc(logfunc), 0);
            lpsolve.print_str(lp, "lp_solve " + Major + "." + Minor + "." + release + "." + build + " demo" + NewLine + NewLine);
            lpsolve.solve(lp); /* just to see that a message is send via the logfunc routine ... */
            /* ok, that is enough, no more callback */
            lpsolve.put_logfunc(lp, null, 0);

            /* set an abort function. Again optional */
            lpsolve.put_abortfunc(lp, new lpsolve.ctrlcfunc(ctrlcfunc), 0);
            /* set a message function. Again optional */
            lpsolve.put_msgfunc(lp, new lpsolve.msgfunc(msgfunc), 0, (int)(lpsolve.lpsolve_msgmask.MSG_PRESOLVE | lpsolve.lpsolve_msgmask.MSG_LPFEASIBLE | lpsolve.lpsolve_msgmask.MSG_LPOPTIMAL | lpsolve.lpsolve_msgmask.MSG_MILPEQUAL | lpsolve.lpsolve_msgmask.MSG_MILPFEASIBLE | lpsolve.lpsolve_msgmask.MSG_MILPBETTER));
#endif

            /* Now redirect all output to a file */
            lpsolve.set_outputfile(lp, LpsolveOutFile);
            lpsolve.print_str(lp,
                              "lp_solve " + Major + "." + Minor + "." + release + "." + build + " demo" + NewLine +
                              NewLine);

            lpsolve.set_timeout(lp, 0);


            // Add constraints
            // pay attention to the 1 base and ignored 0 column for constraints
            int rowIdx = 1;
            row = new double[_variables.Count + 1];
            for (int c = 0; c < _constraintsLE.Count; ++c, rowIdx++)
            {
                SetConstraint(lp, _constraintsLE[c], rowIdx, row,
                              lpsolve.lpsolve_constr_types.LE, string.Format("c_le{0}", c));
            }
            for (int c = 0; c < _constraintsEQ.Count; ++c, rowIdx++)
            {
                SetConstraint(lp, _constraintsEQ[c], rowIdx, row,
                              lpsolve.lpsolve_constr_types.EQ, string.Format("c_eq{0}", c));
            }

            // Set bounds and variable names
            double inf = lpsolve.get_infinite(lp);
            for (int i = 0; i < _variables.Count; ++i)
            {
                string varName = _variables.GetName(i);
                lpsolve.set_col_name(lp, i + 1, varName);
                if (varName.StartsWith("h"))
                {
                    lpsolve.set_lowbo(lp, i + 1, 0);
                    lpsolve.set_upbo(lp, i + 1, +inf);
                }
                else
                {
                    lpsolve.set_lowbo(lp, i + 1, -inf);
                    lpsolve.set_upbo(lp, i + 1, +inf);
                }
            }

            // Set objective function f = v0
            row.Fill(0);
            for (int i = 0; i < _variables.Count; ++i)
            {
                // v0 is always the last, so it's index is Count-1, and 1 is added for lpsolve format.
                row[_v0 + 1] = 1;
            }

            lpsolve.set_obj_fn(lp, row);
            lpsolve.set_maxim(lp);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Now solve the problem with lpsolve.solve(lp);" + NewLine);
            lpsolve55.lpsolve.lpsolve_return solutionStatus = lpsolve.solve(lp);
            lpsolve.print_str(lp, solutionStatus + ": " + lpsolve.get_objective(lp) + NewLine);

            IsLpSolutionFound = solutionStatus == lpsolve55.lpsolve.lpsolve_return.OPTIMAL;

            if (IsLpSolutionFound)
            {
                _solutionCol = new double[lpsolve.get_Ncolumns(lp)];
                lpsolve.get_variables(lp, _solutionCol);
            }
            else
            {
                _solutionCol = null;
            }

            Value = lpsolve.get_objective(lp);

            lpsolve.set_outputfile(lp, null);
            lpsolve.delete_lp(lp);
        }

        void SetConstraint(int lp, Constraint constr, int rowIdx, double[] row, lpsolve.lpsolve_constr_types type, string name)
        {
            row.Fill(0);
            for (int i = 0; i < constr.Vector.Count; ++i)
            {
                row[i + 1] = constr.Vector[i];
            }
            bool result = lpsolve.set_row(lp, rowIdx, row);
            lpsolve.set_constr_type(lp, rowIdx, type);
            lpsolve.set_rh(lp, rowIdx, constr.RightHandSide);
            lpsolve.set_row_name(lp, rowIdx, name);
        }

        private void CopySolutionToStrategy()
        {
            _varH = 0;
            WalkUFTreePP<StrategyTree, CopySolutionToStrategyContext> wt = new WalkUFTreePP<StrategyTree, CopySolutionToStrategyContext>();
            wt.OnNodeBegin = CopySolutionToStrategy_OnNodeBegin;
            wt.Walk(Strategy, PLAYERS_COUNT);
        }

        void CopySolutionToStrategy_OnNodeBegin(StrategyTree tree, CopySolutionToStrategyContext[] stack, int depth)
        {
            CopySolutionToStrategyContext context = stack[depth];
            Int64 n = context.NodeIdx;
            if (depth > PLAYERS_COUNT)
            {
                if (tree.Nodes[n].IsPlayerAction(HeroPosition))
                {
                    tree.Nodes[n].Probab = _solutionCol[_varH];
                    _varH++;
                }
            }
        }

        private void CleanUp()
        {
            // Release large objects.
            _actionTreeIndex = null;
            _heroVarsInLeaves = null;
            _chanceTreeNodes = null;
            _solutionCol = null;
            _oppStrategy = null;
        }

        const int PLAYERS_COUNT = 2;

        int _roundsCount;
        int[][] _maxCard;
        /// <summary>
        /// Numbers of chance indexes for each player and round.
        /// </summary>
        int[][] _chanceIndexSizes;
        /// <summary>
        /// For each node of action tree - an array of hero variables.
        /// </summary>
        int[][] _heroVarsInLeaves;

        /// <summary>
        /// Index of the chance tree, for each round and each possible chance index of the hero contains 
        /// an array with node indexes of the chance tree. For each combination of opponents' cards
        /// there is an entry in this array. The entries that do not correspond to any possible cards are filled 
        /// with 0.
        /// </summary>
        int[][][] _chanceTreeNodes;

        int _oppPosition;
        StrategyTree _oppStrategy; 
        /// <summary>
        /// Children index of the action tree.
        /// </summary>
        UFTreeChildrenIndex _actionTreeIndex;
        protected Variables _variables;
        protected List<Constraint> _constraintsLE;
        protected List<Constraint> _constraintsEQ;
        double[] _solutionCol;
        int _v0;
        int _varH;

        #endregion

        #region lpsolve

#if DEBUG_LPSOLVE
        private static void logfunc(int lp, int userhandle, string Buf)
        {
            System.Diagnostics.Debug.Write(Buf);
        }

        private static bool ctrlcfunc(int lp, int userhandle)
        {
            /* 'If set to true, then solve is aborted and returncode will indicate this. */
            return (false);
        }

        private static void msgfunc(int lp, int userhandle, lpsolve.lpsolve_msgmask message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
#endif

        #endregion
    }
}
