/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.pkr.metagame;
using System.Reflection;
using ai.lib.utils;

namespace ai.pkr.metastrategy.vis
{
    /// <summary>
    /// Base class for poker tree vizualisaion context. Contains properites used 
    /// by visualization algorithm to format the poker tree (node shape, etc.).
    /// Derived classes must fill these properties and can and new properties.
    /// </summary>
    /// <typeparam name="NodeT"></typeparam>
    public class VisPkrTreeContext<NodeT, IteratorT> : VisTreeContext<NodeT, IteratorT>
    {
        #region Properties that must be set by derived classes.

        /// <summary>
        /// Node id. May differ from GvId, for example if the tree is pruned. 
        /// Therefore the implementation has to set it.
        /// </summary>
        public String Id
        {
            set;
            get;
        }

        public bool IsDealerAction
        {
            set;
            get;
        }

        public int Position
        {
            set;
            get;
        }

        public int Round
        {
            set;
            get;
        }

        /// <summary>
        /// Action label, for example 0d1
        /// </summary>
        public string ActionLabel
        {
            set;
            get;
        }

        #endregion

        #region Properties set automatically

        public bool IsGameOver
        {
            set;
            get;
        }

        public int ChildPosition
        {
            set;
            get;
        }


        #endregion

        public override string ToPathString()
        {
            return ActionLabel;
        }
    }

    /// <summary>
    /// Visualizer for poker trees. Poker trees can be build of different data structures, 
    /// but usually contain the similar data. This class uses VisPkrTreeContext as data source 
    /// that must be filled by the user.
    /// <para>To use this class for a custom poker tree:</para>
    /// <para>1. Override OnNodeBegin() and fill the properties of the context with the data from your tree
    /// BEFORE calling base class (because base class can rely on these properties).</para>
    /// <para>2. Call base class function.</para>
    /// <para>3. Do whatever else is necessary and return the appropriate value.</para>
    /// <para>It is recommended to use HTML style (#RRGGBB) for colors to allow automatic shading in some algorithms.
    /// </para>
    /// </summary>
    public class VisPkrTree<TreeT, NodeT, IteratorT, ContextT> : VisTree<TreeT, NodeT, IteratorT, ContextT> where ContextT : VisPkrTreeContext<NodeT, IteratorT>, new()
    {
        #region Public

        public delegate bool PruneIfExtDelegate(TreeT tree, NodeT node, List<ContextT> stack, int depth);

        public VisPkrTree()
        {
            // Add metagame assembly, is will be almost always used in expressions.
            CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(typeof(GameState).Assembly));
            // Add this assembly
            CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));

            GraphAttributes.labeljust = "l";
            GraphAttributes.labelloc = "t";
            GraphAttributes.fontsize = 28;
            GraphAttributes.nodesep = 0.05;

            EdgeAttributes.arrowhead = "none";
        }

        public enum EdgeLabelPlacementKind
        {
            None = 0x00,
            /// <summary> Beginning of the edge. </summary>
            Tail = 0x01,
            /// <summary> Middle of the edge. </summary>
            Middle = 0x02,
            /// <summary> End of the edge. </summary>
            Head = 0x04,
        }

        public EdgeLabelPlacementKind EdgeLabelPlacement
        {
            set { _edgeLabelPlacement = value; }
            get { return _edgeLabelPlacement; }
        }


        public string[] CurrentActorFillColor
        {
            set { _currentActorFillColor = value; }
            get { return _currentActorFillColor; }
        }

        /// <summary>
        /// An extended version of PruneIf, will be called after initializing the context.
        /// If it returns true, the tree is pruned at this node.
        /// </summary>
        public PruneIfExtDelegate PruneIfExt
        {
            set;
            get;
        }

        // Todo: support this if necessary
        //public List<PokerAction> ActionPath
        //{
        //    set;
        //    get;
        //}

        #endregion

        #region Overriden from base classes

        /// <summary>
        /// Round name for GV, replaces round -1 by "B", otherwise GV cannot work with it.
        /// </summary>
        /// <returns></returns>
        protected static string GetGvRoundName(int round)
        {
            string roundName = round >= 0 ? round.ToString() : "B";
            return roundName;
        }

        protected override void OnTreeEndFunc(TreeT tree, NodeT root)
        {
            NodeAttributeMap na = new NodeAttributeMap();
            na.fontsize = 22;
            na.style = "bold";
            na.peripheries = 0;

            EdgeAttributeMap ea = new EdgeAttributeMap();
            ea.style = "invis";

            _output.WriteLine("{");

            for (int r = -1; r <= _maxRound; ++r)
            {
                string roundName = GetGvRoundName(r);
                na.label = "Round " + r.ToString();
                WriteNode("round" + roundName, na);
                if (r > 0)
                {
                    string prevRoundName = GetGvRoundName(r - 1);
                    WriteEdge("round" + prevRoundName.ToString(), "round" + roundName, ea);
                }
            }
            _output.WriteLine("}");

            base.OnTreeEndFunc(tree, root);
        }

        protected override bool OnNodeBeginFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            bool result = base.OnNodeBeginFunc(tree, node, stack, depth);
            // Set context attributes here. This is the best place, because 
            // some child attributes influence parents, and we can assure that
            // the children will be visited here even if the have to be ignored
            // in the visualization.
            ContextT context = stack[depth];

            context.IsGameOver = true;
            context.ChildPosition = -1;
            if (depth > 0)
            {
                stack[depth - 1].IsGameOver = false;
                stack[depth - 1].ChildPosition = context.Position;
            }

            if(result &&  PruneIfExt != null && PruneIfExt(tree, node, stack, depth))
            {
                result = false;
            }
            return result;
        }

        protected override void OnNodeEndFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            // On node end State is already assigned, so we can calculate the _maxRound
            _maxRound = Math.Max(stack[depth].Round, _maxRound);
            base.OnNodeEndFunc(tree, node, stack, depth);
        }

        /// <summary>
        /// Completely overrides base class to align nodes of one round.
        /// </summary>
        protected override void VisNode(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            ContextT context = stack[depth];
            VisNodeImpl(tree, node, stack, depth);
        }

        private void VisNodeImpl(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            NodeAttributeMap attr = new NodeAttributeMap();
            bool isNewRound = false;
            if (depth == 0 || stack[depth - 1].Round < stack[depth].Round)
            {
                isNewRound = true;
                string roundName = GetGvRoundName(stack[depth].Round);
                _output.WriteLine("{{rank=same; round{0};", roundName);
            }
            CustomizeNodeAttributes(tree, node, stack, depth, attr);
            WriteNode(stack[depth].GvId, attr);
            if (isNewRound)
            {
                _output.WriteLine("}");
            }
        }

        protected override void CustomizeNodeAttributes(TreeT tree, NodeT node, List<ContextT> stack, int depth, NodeAttributeMap attr)
        {
            ContextT context = stack[depth];

            attr.label = GetNodeLabel(tree, node, stack, depth);

            if (context.ChildPosition >= 0 && context.ChildPosition < CurrentActorFillColor.Length)
            {
                attr.fillcolor = CurrentActorFillColor[context.ChildPosition];
            }
            else
            {
                attr.fillcolor = "#FFFFFF";
            }

            attr.style = "filled";

            if (context.IsGameOver)
            {
                attr.shape = "box";
            }
            else
            {
                if (context.IsDealerAction)
                {
                    attr.shape = "ellipse";
                }
                else
                {
                    attr.shape = "box";
                }
            }
            if (IsInActionPath(stack, depth))
            {
                attr.style = "bold";
            }
        }

        protected override void VisEdge(TreeT tree, NodeT node, NodeT parent, List<ContextT> stack, int depth)
        {
            ContextT context = stack[depth];
            base.VisEdge(tree, node, parent, stack, depth);
        }


        protected override void CustomizeEdgeAttributes(TreeT tree, NodeT node, NodeT parent, List<ContextT> stack, int depth, EdgeAttributeMap attr)
        {
            ContextT context = stack[depth];
            string label = context.ActionLabel;
            //if (context.IsLastPrivateDealInARow)
            //{
            //    for (int d = depth - 1; d >= 0 && stack[d].Action.Kind == Ak.d; d--)
            //    {
            //        label = stack[d].Action.ToString() + " " + label;
            //    }
            //}
            if ((EdgeLabelPlacement & EdgeLabelPlacementKind.Head) != 0)
                attr.headlabel = label;
            if ((EdgeLabelPlacement & EdgeLabelPlacementKind.Middle) != 0)
                attr.label = label;
            if ((EdgeLabelPlacement & EdgeLabelPlacementKind.Tail) != 0)
                attr.taillabel = label;
        }

        #endregion

        #region Protected virtual functions

        ///// <summary>
        ///// Override to customize the node label.
        ///// </summary>
        protected virtual string GetNodeLabel(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            return EvalExpressions(tree, stack, depth);
        }

        #endregion

        #region Protected API.

        protected bool IsInActionPath(List<ContextT> stack, int depth)
        {
            //if (ActionPath == null || ActionPath.Count < depth)
            //    return false;

            //for (int i = 0; i < depth; ++i)
            //{
            //    if (!ActionPath[i].Equals(stack[i].Action))
            //        return false;
            //}
            return false;
        }

        protected int _maxRound = -1;

        #endregion

        #region Private

        private EdgeLabelPlacementKind _edgeLabelPlacement = EdgeLabelPlacementKind.Middle;
        private string[] _currentActorFillColor = new string[] { "#FFD0D0", "#D0FFD0", "#D0D0FF", "#FFFFD0", "#FFD0FF" };
        #endregion
    }

    /// <summary>
    /// A shortcut for VisPkrTree&lt;...&gt; that uses the standard type for the context.
    /// </summary>
    public class VisPkrTree<TreeT, NodeT, IteratorT> : VisPkrTree<TreeT, NodeT, IteratorT, VisPkrTreeContext<NodeT, IteratorT>>
    {
    }

    /// <summary>
    /// A shortcut for VisPkrTree&lt;...&gt; for most typical usecase where TreeT == NodeT and standard context is used.
    /// </summary>
    public class VisPkrTree<NodeT, IteratorT> : VisPkrTree<NodeT, NodeT, IteratorT, VisPkrTreeContext<NodeT, IteratorT>>
    {
    }
}
