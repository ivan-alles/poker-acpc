/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;
using ai.pkr.metagame;
using System.Diagnostics;
using System.IO;
using ai.lib.algorithms.numbers;
using System.Drawing;

namespace ai.pkr.metastrategy
{

    /// <summary>
    /// Calculate best response against a static strategy. 
    /// <para>Usage:</para>
    /// <para>1. Create an instance, set GameDef and HeroPosition</para>
    /// <para>2. Call CreateTrees() to create tree structures.</para>
    /// <para>3. For all opponents - set StrategicProbability in PlayerTrees</para>
    /// <para>4. Call calculate. </para>
    /// <para>5. Read the result in PlayerTrees and GameTree, use Visualizer to view the results.</para>
    /// </summary>
    /// <remarks>
    /// <para>Algorithm:</para>
    /// <para>1. Create a game tree and player trees for each player.</para>
    /// <para>2. Let the caller to set strategic probabilities for all players except hero.</para>
    /// <para>3. For each villain's tree: traverse game tree and villains tree in parallel,
    /// assign and validate strategic probablities.</para>
    /// <para>4. Traverse game tree and hero's tree in parallel, calc. values in term node,
    /// store them in game tree, accumulate in hero's tree.</para>
    /// <para>5. Traverse hero's tree in postorder marking best response edges and calculating game values.
    /// After this step the best response proper is known.</para>
    /// <para>6. Traverse trough the game tree and hero's tree again, taking best edge,
    /// store the values and probabilities. Mark best response edges here as well. 
    /// Here we also keep average values in terminal nodes even if
    /// there have not been played by the hero.</para>
    /// <para>7. Traverse through game tree and opponent trees, accumulate values there. 
    /// The values are from the point of view of the hero. Here we never go to non-best edges of the hero.</para>
    /// </remarks>
    public class BestResponseStatic
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
                BestActionIndex = -1;
                StrategicProbab = 1;
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
            /// Absolute probability of the player chosing this node in the game tree.
            /// </summary>
            public double StrategicProbab
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

            /// <summary>
            /// Absolute probability of playing this node.
            /// </summary>
            public double Probab
            {
                set;
                get;
            }

            /// <summary>
            /// Game value.
            /// </summary>
            public double Value
            {
                set;
                get;
            }

            /// <summary>
            /// Average value. Is set explicitely in order to allow to show game result values 
            /// in terminal nodes of the game tree that are never played (Probab == 0).
            /// </summary>
            public double AverageValue
            {
                set;
                get;
            }

            /// <summary>
            /// For the hero in nodes where he acts: index of the best action in the children container,
            /// in other nodes: -1.
            /// </summary>
            public int BestActionIndex
            {
                set;
                get;
            }

            public List<TreeNode> Children
            {
                internal set;
                get;
            }

            public override string ToString()
            {
                return String.Format("{0} v:{1} p:{2} av: {3} bai: {4}", Action, Value, Probab, AverageValue, BestActionIndex);
            }
        }

        /// <summary>
        /// Visualizes one of the trees that is the result of this algorithm.
        /// </summary>
        public class Visualizer : VisPokerTree<TreeNode, TreeNode, int, Visualizer.Context>
        {
            public class Context : VisPokerTreeContext<TreeNode, int>
            {
                /// <summary>
                /// Is set to false if the hero never choses this node (or one of its parents).
                /// </summary>
                public bool IsPlayed;
            }

            /// <summary>
            /// Sets default attributes of the graph, such as graph label and expressions to show.
            /// </summary>
            /// <param name="perspectivePos">Show perspective of the given player. Set to a negative value to show the game tree.</param>
            public virtual void SetDefaultAttrbutes(BestResponseStatic br, int perspectivePos)
            {
                string label = string.Format("{0}, best response pos {1}", br.GameDef.Name, br.HeroPosition);
                if (perspectivePos >= 0)
                {
                    label += String.Format(", perspective pos {0}", perspectivePos);
                }
                GraphAttributes.label = label;
                ShowExprFromString(new string[]
                                          {
                                              "s[d].Node.Id;id:{1}",
                                              "s[d].Node.Value;\\nv:{1:0.#####}",
                                              "s[d].Node.AverageValue;\\na:{1:0.#####}",
                                              "s[d].Node.Probab;\\np:{1:0.#####}"
                                          });
            }

            protected override bool OnNodeBeginFunc(TreeNode tree, TreeNode node,
                List<Context> stack, int depth)
            {
                Context context = stack[depth];

                if(depth > 0)
                {
                    TreeNode parent = stack[depth - 1].Node;
                    if (parent.BestActionIndex >= 0 && stack[depth - 1].IsPlayed)
                    {
                        context.IsPlayed = parent.BestActionIndex == stack[depth - 1].ChildrenIterator - 1;
                    }
                    else
                    {
                        context.IsPlayed = stack[depth - 1].IsPlayed;
                    }
                }
                else
                {
                    context.IsPlayed = true;
                }

                context.Action = node.Action;
                context.State = node.State;

                return base.OnNodeBeginFunc(tree, node, stack, depth);
            }

            byte Gradient(byte col1, byte col2, double gr)
            {
                double result = col1*gr + col2*(1 - gr);
                return (byte)(Math.Min(255, result));
            }

            protected override void CustomizeNodeAttributes(TreeNode tree, TreeNode node, List<Visualizer.Context> stack, int depth, VisTree<TreeNode, TreeNode, int, Visualizer.Context>.NodeAttributeMap attr)
            {
                base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
                if(!stack[depth].IsPlayed)
                {
                    string fillcolor = attr.fillcolor;
                    if(!string.IsNullOrEmpty(fillcolor) && fillcolor.StartsWith("#") && fillcolor.Length == 7)
                    {
                        // Color format #RRGGBB
                        Color c = ColorTranslator.FromHtml(fillcolor);
                        byte grayed = 200;
                        double grad = 0.4;
                        byte r = Gradient(c.R, grayed, grad);
                        byte g = Gradient(c.G, grayed, grad);
                        byte b = Gradient(c.B, grayed, grad);
                        Color nc = Color.FromArgb(r, g, b);
                        attr.fillcolor = ColorTranslator.ToHtml(nc);
                    }
                }
            }

            protected override void CustomizeEdgeAttributes(TreeNode tree, TreeNode node, TreeNode parent, List<Context> stack, int depth, EdgeAttributeMap attr)
            {
                base.CustomizeEdgeAttributes(tree, node, parent, stack, depth, attr);
                // Hero have chosen this edge - show thick
                if (stack[depth - 1].Node.BestActionIndex == stack[depth - 1].ChildrenIterator - 1)
                {
                    attr.penwidth = 2.0;
                }
            }
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

        public TreeNode GameTree
        {
            get;
            protected set;
        }

        public TreeNode[] PlayerTrees
        {
            get;
            protected set;
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

        #endregion 

        #region Public methods

        public BestResponseStatic()
        {
            CreateRootGenNode = CreateRootGenNodeFunc;
        }

        /// <summary>
        /// Call it to create tree structure.
        /// </summary>
        public void CreateTrees()
        {
            Step1();
        }

        /// <summary>
        /// Call it after calling CreateTrees() and setting strategic probabilities of the opponents.
        /// </summary>
        public void Calculate()
        {
            Step3();
            Step4();
            Step5();
            Step6();
            Step7();
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
        #endregion

        #region Step 3

        protected int _step3CurPos;

        private void Step3()
        {
            for (_step3CurPos = 0; _step3CurPos < PlayerTrees.Length; ++_step3CurPos)
            {
                if (_step3CurPos == HeroPosition)
                    continue;
                WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
                walkTree.OnNodeBegin = Step3_OnNodeBegin;
                walkTree.Walk(GameTree, GameTree);
            }
        }


        protected bool Step3_OnNodeBegin(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            Context context = stack[depth];

            SyncPlayerTreeWithGameTree(stack, depth, _step3CurPos);

            TreeNode playerNode = context.PlayerTreeNode;

            // Correct and validate strategy of the opponent.
            if (depth == 0)
            {
                playerNode.StrategicProbab = 1;
            }
            else if (!node.State.IsGameOver)
            {
                if (node.State.IsPlayerActing(_step3CurPos))
                {
                    // Sum of strategic probability of children must be equal to the str. prob. of the parent.
                    double StrategicProbabOfChildren = 0;
                    for (int c = 0; c < playerNode.Children.Count; ++c)
                    {
                        StrategicProbabOfChildren += playerNode.Children[c].StrategicProbab;
                    }

                    if (
                        !FloatingPoint.AreEqualRel(playerNode.StrategicProbab, StrategicProbabOfChildren,
                                                   0.000001))
                    {
                        throw new ApplicationException(
                            string.Format(
                                "Player {0}, node id {1} : strategic probab. {2} != sum of strategic probab. of children {3}",
                                _step3CurPos, playerNode.Id,
                                playerNode.StrategicProbab,
                                StrategicProbabOfChildren));
                    }
                }
                else
                {
                    // Strategic probability of each child must be the same as of the parent.
                    for (int c = 0; c < playerNode.Children.Count; ++c)
                    {
                        playerNode.Children[c].StrategicProbab = playerNode.StrategicProbab;
                    }
                }
            }
            node.StrategicProbab *= playerNode.StrategicProbab;
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

            SyncPlayerTreeWithGameTree(stack, depth, HeroPosition);

            TreeNode playerNode = context.PlayerTreeNode;

            if (node.State.IsGameOver)
            {
                if (node.State.IsShowdownRequired)
                {
                    int[] ranks = GameDef.GameRules.Showdown(GameDef, node.State);
                    node.State.UpdateByShowdown(ranks, GameDef);
                }
                double result = node.State.Players[HeroPosition].Result;
                double probab = node.StrategicProbab * node.ChanceProbab;
                double value = result * probab;

                node.Value = value;
                node.Probab = probab;
                node.AverageValue = result;

                playerNode.Value += value;
                playerNode.Probab += probab;
            }

            return true;
        }

        #endregion

        #region Step 5

        private void Step5()
        {

            WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
            walkTree.OnNodeEnd = Step5OnNodeEnd;
            walkTree.Walk(PlayerTrees[HeroPosition], PlayerTrees[HeroPosition]);
        }

        protected void Step5OnNodeEnd(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            Context context = stack[depth];

            if (node.Children.Count > 0)
            {
                if (node.State.IsPlayerActing(HeroPosition))
                {
                    // Take the best value.
                    TreeNode firstChild = node.Children[0];
                    node.Value = firstChild.Value;
                    node.Probab = firstChild.Probab;
                    node.BestActionIndex = 0;

                    for (int c = 1; c < node.Children.Count; ++c)
                    {
                        TreeNode child = node.Children[c];
                        if (node.Value < child.Value)
                        {
                            node.Value = child.Value;
                            node.Probab = child.Probab;
                            node.BestActionIndex = c;
                        }
                    }
                }
                else
                {
                    // Sum all values.
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        TreeNode child = node.Children[c];
                        node.Value += child.Value;
                        node.Probab += child.Probab;
                    }
                }
            }
            if (node.Probab != 0)
            {
                node.AverageValue = node.Value / node.Probab;
            }
        }

        #endregion

        #region Step 6

        protected void Step6()
        {
            WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
            walkTree.OnNodeBegin = Step6OnNodeBegin;
            walkTree.OnNodeEnd = Step6OnNodeEnd;
            walkTree.Walk(GameTree, GameTree);
        }


        protected bool Step6OnNodeBegin(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            SyncPlayerTreeWithGameTree(stack, depth, HeroPosition);
            return true;
        }

        protected void Step6OnNodeEnd(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            Context context = stack[depth];

            if (node.Children.Count > 0)
            {
                if (node.State.IsPlayerActing(HeroPosition))
                {
                    // Take the best value.
                    node.BestActionIndex = context.PlayerTreeNode.BestActionIndex;
                    TreeNode child = node.Children[node.BestActionIndex];
                    node.Value = child.Value;
                    node.Probab = child.Probab;
                }
                else
                {
                    // Sum all values.
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        TreeNode child = node.Children[c];
                        node.Value += child.Value;
                        node.Probab += child.Probab;
                    }
                }
                if (node.Probab != 0)
                {
                    node.AverageValue = node.Value / node.Probab;
                    // Average for terminal nodes is already set.
                }
            }
        }

        #endregion

        #region Step 7

        protected int _step7CurPos;

        protected void Step7()
        {
            for (_step7CurPos = 0; _step7CurPos < PlayerTrees.Length; ++_step7CurPos)
            {
                if (_step7CurPos == HeroPosition)
                    continue;
                WalkTree<TreeNode, TreeNode, int, Context> walkTree = new WalkTree<TreeNode, TreeNode, int, Context>();
                walkTree.OnNodeBegin = Step7_OnNodeBegin;
                walkTree.Walk(GameTree, GameTree);
            }
        }


        protected bool Step7_OnNodeBegin(TreeNode tree, TreeNode node, List<Context> stack, int depth)
        {
            if (node.State.HasPlayerActed(HeroPosition) && 
                stack[depth - 1].ChildrenIterator - 1 != stack[depth - 1].Node.BestActionIndex)
            {
                // Do not go to non-best response edges when hero acted.
                return false;
            }
            SyncPlayerTreeWithGameTree(stack, depth, _step7CurPos);
            Context context = stack[depth];
            TreeNode playerNode = context.PlayerTreeNode;

            playerNode.Value += node.Value;
            playerNode.Probab += node.Probab;
            if(playerNode.Probab != 0)
            {
                playerNode.AverageValue = playerNode.Value/playerNode.Probab;
            }

            return true;
        }


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
                else if(node.Action.Kind == Ak.d && node.State.LastActor != curPos)
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
