using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.algorithms.tree;
using System.IO;
using System.Drawing;
using System.Globalization;
using ai.lib.algorithms;
using lpsolve55;
using System.Diagnostics;
using ai.lib.utils;
using ai.pkr.metastrategy;

namespace equilibrium_lp
{
    /// <summary>
    /// Prepares data for lp equilibrium article.
    /// Was originally copied from ai.pkr.metastrategy.EquilibriumSolverLp and adjusted.
    /// </summary>
    /// <para>Algorithm:</para>
    /// <para>1. Create a game tree and player trees for each player.</para>
    /// <para>2. Traverse hero's tree, add variables for nodes where he acts, add constraints on them.</para>
    /// <para>3. Traverse game tree and hero's tree in parallel, store terminal variables from hero's tree to game tree.</para>
    /// <para>4. Traverse villain's tree and game tree. In game tree, calculate game values in terminal nodes. In player tree
    /// accumulate these values.</para>
    /// <para>5. Traverse villain's tree and add v-variables, r-variables and corresponding constraints.</para>
    /// <para>6. Formulate and solve lp.</para>
    /// <para>7. If solution is found, store it in the Solution array in sparse absolute strategy format.</para>
    /// </remarks>
    public class EquilibriumSolverLp
    {
        #region Public classes

        /// <summary>
        /// Creates a root node for tree generator.
        /// </summary>
        public delegate GenNode CreateRootGenNodeDelegate(GenTree tree);

        public class TreeNode
        {
            public static bool TreeGetChild(TreeNode tree, TreeNode n, ref int i, out TreeNode child)
            {
                return (i < n.Children.Count ? child = n.Children[i++] : child = null) != null;
            }

            public TreeNode()
            {
                Children = new List<TreeNode>();
                Var_h = -1;
                Var_v = -1;
            }

            public int Id;

            public PokerAction Action
            {
                set;
                get;
            }

            public GameState State
            {
                set;
                get;
            }



            /// <summary>
            /// Absolute probability determined by chance factor of the game. Depends on game defininition
            /// only, is not influenced by strategies of players.
            /// </summary>
            public double ChanceProbab
            {
                set;
                get;
            }

            public List<TreeNode> Children
            {
                internal set;
                get;
            }

            /// <summary>
            /// Index of h variable for player tree of the hero. Is propagated from the node
            /// where the hero acted down to the children where he is not acted.
            /// </summary>
            public int Var_h
            {
                set;
                get;
            }

            /// <summary>
            /// Index of v variable in opponent tree.
            /// </summary>
            public int Var_v
            {
                set;
                get;
            }

            /// <summary>
            /// In terminal nodes of villains tree - coeficients for h-variables.
            /// </summary>
            public List<double> TerminalCoeffs_h
            {
                set;
                get;
            }

            /// <summary>
            /// In terminal nodes of villains tree - indexes for h-variables.
            /// </summary>
            public List<int> TerminalVars_h
            {
                set;
                get;
            }


            public override string ToString()
            {
                return String.Format("{0}", Action);
            }
        }

        /// <summary>
        /// LP variables.
        /// </summary>
        public class Variables
        {
            public int Add(string type, int nodeId)
            {
                string name = type + nodeId.ToString();
                int ordinal = _list.Count;
                _list.Add(name);
                return ordinal;
            }

            public int FindIndex(string name)
            {
                for (int i = 0; i < _list.Count; ++i)
                {
                    if (_list[i] == name)
                        return i;
                }
                return -1;
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
        public class Constraint
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


        #endregion

        #region Public properties

        public GameDefinition GameDef
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
        /// Creates a root generator node. By default is set to a protected function CreateRootGenNodeFunc 
        /// which creates a DeckGenNode.
        /// You can either override the function or set this delegate to use a custom node, for instance 
        /// a NormSuitGenNode.
        /// </summary>
        public CreateRootGenNodeDelegate CreateRootGenNode
        {
            set;
            get;
        }

        public TreeNode GameTree
        {
            get;
            set;
        }

        public TreeNode[] PlayerTrees
        {
            get;
            set;
        }

        public Variables Vars = new Variables();

        #endregion

        #region Public methods

        public EquilibriumSolverLp()
        {
            CreateRootGenNode = CreateRootGenNodeFunc;
        }

        /// <summary>
        /// Call it after calling CreateTrees() and setting strategic probabilities of the opponents.
        /// </summary>
        public void Calculate()
        {
            if(GameDef.MinPlayers != 2)
            {
                throw new ApplicationException(
                    String.Format("Only 2 player games can be solved, {0} is a {1}-player",
                    GameDef.Name, GameDef.MinPlayers));
            }
            Step1();
            Step2();
            Step3();
            Step4();
            Step5();
        }

        /// <summary>
        /// Prints the problem in CPLEX format.
        /// </summary>
        public void PrintCPLEX(TextWriter wr)
        {
            wr.WriteLine("\\\\Vars: {0}, constr le: {1}, constr eq: {2}",
                         Vars.Count, _constraintsLE.Count, _constraintsEQ.Count);
            wr.Write("\\\\Vars:");
            for (int i = 0; i < Vars.Count; ++i)
            {
                wr.Write(Vars.GetName(i));
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
                wr.WriteLine(_constraintsLE[i].ToString(Vars, " <= "));
            }
            wr.WriteLine();
            wr.WriteLine("\\\\Constraints equal:");
            for (int i = 0; i < _constraintsEQ.Count; ++i)
            {
                wr.Write("c_eq{0}:  ", i);
                wr.WriteLine(_constraintsEQ[i].ToString(Vars, "  = "));
            }
            wr.WriteLine();
            wr.WriteLine("Bounds");
            for (int i = 0; i < Vars.Count; ++i)
            {
                string varName = Vars.GetName(i);
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


        #region Protected classes

        
        protected class GenContext : WalkTreeContext<GenNode, int>
        {
            public TreeNode SecondTreeNode
            {
                get;
                set;
            }
        }

        protected class Context : WalkTreeContext<TreeNode, int>
        {
            public TreeNode PlayerTreeNode
            {
                get;
                set;
            }

            /// <summary>
            /// For each node in hero's player tree shows if hero has acted at least once
            /// in the path from the root to this node.
            /// </summary>
            public bool HasHeroActedAtLeastOnce
            {
                set;
                get;
            }
        }

        #endregion

        #region Step 1

        protected TreeNode _step1Root;

        private void Step1()
        {
            GenTree tree = new GenTree { GameDef = GameDef, Kind = GenTree.TreeKind.GameTree };
            GenNode root = CreateRootGenNode(tree);
            WalkTree<GenTree, GenNode, int, GenContext> walkTree = new WalkTree<GenTree, GenNode, int, GenContext>();
            walkTree.OnNodeBegin = Step1OnNodeBegin;
            GameTree = new TreeNode();
            _step1Root = GameTree;
            walkTree.Walk(tree, root);


            PlayerTrees = new TreeNode[GameDef.MinPlayers].Fill(i => new TreeNode());
            for (int p = 0; p < PlayerTrees.Length; ++p)
            {
                tree = new GenTree { GameDef = GameDef, Kind = GenTree.TreeKind.PlayerTree, HeroPosition = p };
                root = CreateRootGenNode(tree);
                walkTree = new WalkTree<GenTree, GenNode, int, GenContext>();
                walkTree.OnNodeBegin = Step1OnNodeBegin;
                _step1Root = PlayerTrees[p];
                walkTree.Walk(tree, root);
            }
        }

        protected bool Step1OnNodeBegin(GenTree tree, GenNode node, List<GenContext> stack, int depth)
        {
            GenContext context = stack[depth];

            TreeNode newNode = depth == 0 ? _step1Root : new TreeNode();

            newNode.Id = node.Id;
            newNode.State = node.State;
            newNode.Action = node.Action;
            newNode.ChanceProbab = node.ChanceProbab;


            if (depth > 0)
            {
                GenContext parentContext = stack[depth - 1];
                parentContext.SecondTreeNode.Children.Add(newNode);
            }
            context.SecondTreeNode = newNode;
            return true;
        }

        /// <summary>
        /// Creates a root GenNode-derivation to build tree structure. 
        /// Override to use a custom GenNode subclass. 
        /// </summary>
        protected virtual GenNode CreateRootGenNodeFunc(GenTree gameTree)
        {
            return new DeckGenNode(gameTree, GameDef.MinPlayers);
        }

        #endregion

        #region Step 2

        private void Step2()
        {
            WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
            walkTree.OnNodeBegin = Step2_OnNodeBegin;
            _heroNodeCount = 0;
            walkTree.Walk(PlayerTrees[HeroPosition], PlayerTrees[HeroPosition]);
        }

        protected bool Step2_OnNodeBegin(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            Context context = stack[depth];
            _heroNodeCount++;

            if (node.State.HasPlayerActed(HeroPosition))
            {
                context.HasHeroActedAtLeastOnce = true;
            }
            else if (depth > 0)
            {
                context.HasHeroActedAtLeastOnce = stack[depth - 1].HasHeroActedAtLeastOnce;
                node.Var_h = stack[depth - 1].Node.Var_h;
            }

            if (node.State.IsPlayerActing(HeroPosition))
            {
                Constraint constr = new Constraint();
                if (node.Var_h == -1)
                {
                    // This is the very first move of the hero in this tree branch,
                    // add constraint Sum(h_child) = 1 
                    constr.RightHandSide = 1;
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        node.Children[c].Var_h = Vars.Add("h", node.Children[c].Id);
                        constr.AddVariable(node.Children[c].Var_h, 1);
                    }
                }
                else
                {
                    // Add constraint h_prev_move = Sum(h_child)
                    constr.RightHandSide = 0;
                    constr.AddVariable(node.Var_h, -1);
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        node.Children[c].Var_h = Vars.Add("h", node.Children[c].Id);
                        constr.AddVariable(node.Children[c].Var_h, 1);
                    }
                }
                _constraintsEQ.Add(constr);
            }
            return true;
        }

        #endregion

        #region Step 3


        private void Step3()
        {
            WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
            walkTree.OnNodeBegin = Step3_OnNodeBegin;
            walkTree.Walk(GameTree, GameTree);
        }


        protected bool Step3_OnNodeBegin(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            Context context = stack[depth];
            SyncPlayerTreeWithGameTree(stack, depth, HeroPosition);
            TreeNode playerNode = context.PlayerTreeNode;
            if (node.State.IsGameOver)
            {
                node.Var_h = playerNode.Var_h;
            }
            return true;
        }

        #endregion

        #region Step 4

        protected void Step4()
        {
            WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
            walkTree.OnNodeBegin = Step4_OnNodeBegin;
            walkTree.Walk(GameTree, GameTree);
        }


        protected bool Step4_OnNodeBegin(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            Context context = stack[depth];

            SyncPlayerTreeWithGameTree(stack, depth, 1 - HeroPosition);

            TreeNode playerNode = context.PlayerTreeNode;

            if (node.State.IsGameOver)
            {
                if (node.State.IsShowdownRequired)
                {
                    int[] ranks = GameDef.GameRules.Showdown(GameDef, node.State);
                    node.State.UpdateByShowdown(ranks, GameDef);
                }
                double result = node.State.Players[1 - HeroPosition].Result;
                result *= node.ChanceProbab;
                if (playerNode.TerminalCoeffs_h == null)
                {
                    playerNode.TerminalCoeffs_h = new List<double>();
                    playerNode.TerminalVars_h = new List<int>();
                }

                playerNode.TerminalCoeffs_h.Add(-result);
                playerNode.TerminalVars_h.Add(node.Var_h);

                // Add the values to the game tree too to illustrate the algorithm.
                if (node.TerminalCoeffs_h == null)
                {
                    node.TerminalCoeffs_h = new List<double>();
                    node.TerminalVars_h = new List<int>();
                }
                node.TerminalCoeffs_h.Add(-result);
                node.TerminalVars_h.Add(node.Var_h);
            }

            return true;
        }

        #endregion

        #region Step 5

        private void Step5()
        {

            WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
            walkTree.OnNodeEnd = Step5OnNodeEnd;
            walkTree.Walk(PlayerTrees[1 - HeroPosition], PlayerTrees[1 - HeroPosition]);
        }

        protected void Step5OnNodeEnd(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            Context context = stack[depth];

            if (node.Children.Count == 0)
            {
                // Terminal nodes, add new variables and constraints:
                // v6 = res6 * t14 + res15 * t23
                node.Var_v = Vars.Add("v", node.Id);
                Constraint constr = new Constraint();
                constr.RightHandSide = 0;
                for (int i = 0; i < node.TerminalCoeffs_h.Count; i++)
                {
                    constr.AddVariable(node.TerminalVars_h[i], node.TerminalCoeffs_h[i]);
                }
                constr.AddVariable(node.Var_v, -1);
                _constraintsEQ.Add(constr);
            }
            else if (node.State.IsPlayerActing(1 - HeroPosition))
            {
                //v2 = r3 + r8
                //r3 + r8 <= v3
                //r3 + r8 <= v8

                // Add new variables: v_i and r_*
                node.Var_v = Vars.Add("v", node.Id);
                List<int> var_r = new List<int>();
                for (int i = 0; i < node.Children.Count; i++)
                {
                    int rIdx = Vars.Add("r", node.Children[i].Id);
                    var_r.Add(rIdx);
                }

                // Add 1 eq constraint 
                Constraint constr = new Constraint();
                constr.RightHandSide = 0;
                for (int i = 0; i < var_r.Count; ++i)
                {
                    constr.AddVariable(var_r[i], 1);
                }
                constr.AddVariable(node.Var_v, -1);
                _constraintsEQ.Add(constr);


                // For each child add a le constraint
                for (int i = 0; i < node.Children.Count; i++)
                {
                    Constraint constrLE = new Constraint();
                    constrLE.RightHandSide = 0;
                    for (int j = 0; j < var_r.Count; ++j)
                    {
                        constrLE.AddVariable(var_r[j], 1);
                    }
                    constrLE.AddVariable(node.Children[i].Var_v, -1);
                    _constraintsLE.Add(constrLE);
                }
            }
            else
            {
                // Sum all the children:
                // v3 = v4 + v5
                node.Var_v = Vars.Add("v", node.Id);
                Constraint constr = new Constraint();
                constr.RightHandSide = 0;
                for (int i = 0; i < node.Children.Count; i++)
                {
                    constr.AddVariable(node.Children[i].Var_v, 1);
                }
                constr.AddVariable(node.Var_v, -1);
                _constraintsEQ.Add(constr);
            }

        }

        

        #endregion

        #region Protected fields and properties

        
        protected List<Constraint> _constraintsLE = new List<Constraint>();
        protected List<Constraint> _constraintsEQ = new List<Constraint>();
        protected int _heroNodeCount = 0;

        #endregion


        #region Protected methods 






        /// <summary>
        /// Syncronizes a player tree with the game tree when the latter is being traversed.
        /// </summary>
        protected void SyncPlayerTreeWithGameTree(List<Context> stack, int depth, int curPos)
        {
            Context context = stack[depth];
            TreeNode node = context.Node;
            if (depth == 0)
            {
                context.PlayerTreeNode = PlayerTrees[curPos];
            }
            else
            {
                int childIdx = -1;
                if (node.Action.IsPlayerAction())
                {
                    // For player actions both trees are the same.
                    childIdx = stack[depth - 1].ChildrenIterator - 1;
                }
                else if (node.Action.Kind == Ak.d && node.State.LastActor != curPos)
                {
                    // In private deal action of other players the player tree has only one child.
                    childIdx = 0;
                }
                else
                {
                    // In private deal actions of the current player or in public deal actions 
                    // we have to find the corresponding child.
                    for (childIdx = 0; childIdx < stack[depth - 1].PlayerTreeNode.Children.Count; childIdx++)
                    {
                        if (node.Action.Equals(stack[depth - 1].PlayerTreeNode.Children[childIdx].Action))
                            break;
                    }
                    // If no child is found (should never happen), we will get an index out of range later.
                }
                context.PlayerTreeNode = stack[depth - 1].PlayerTreeNode.Children[childIdx];
            }
        }

        #endregion
    }

}
