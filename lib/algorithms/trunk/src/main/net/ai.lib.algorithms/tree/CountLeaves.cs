/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Count leaves (terminal nodes) in a tree.
    /// </summary>
    public class CountLeaves<TreeT, NodeT, IteratorT> : WalkTreePP<TreeT, NodeT, IteratorT, CountLeaves<TreeT, NodeT, IteratorT>.Context>
    {
        #region Public

        public class Context: WalkTreePPContext<NodeT, IteratorT>
        {
            public bool IsTerminal;
        }

        /// <summary>
        /// After call to Walk() or Count() contains number of leaves.
        /// </summary>
        public int LeavesCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// Counts nodes and returns the number.
        /// </summary>
        public int Count(TreeT tree, NodeT root)
        {
            Walk(tree, root);
            return LeavesCount;
        }

        #endregion

        #region Implementation

        protected override void OnTreeBeginFunc(TreeT tree, NodeT root)
        {
            LeavesCount = 0;
        }

        protected override bool OnNodeBeginFunc(TreeT tree, NodeT node, List<Context> stack, int depth)
        {
            stack[depth].IsTerminal = true;
            return true;
        }

        protected override void OnNodeEndFunc(TreeT tree, NodeT node, List<Context> stack, int depth)
        {
            if (depth > 0)
            {
                stack[depth - 1].IsTerminal = false;
            }
            if (stack[depth].IsTerminal)
            {
                LeavesCount++;
            }
        }


        #endregion
    }

    /// <summary>
    /// Contains static helper functions that use CountLeaves&lt;...&gt;.
    /// They are easier to use, but they are slower in repeated searches 
    /// than creating and using an instance of CountLeaves&lt;&gt;.
    /// Only iterator type is a generic parameter of the class,
    /// the other generic parameters can be deduced by the compiler.
    /// </summary>
    public class CountLeaves<IteratorT> 
    {
        /// <summary>
        /// Count leaves.
        /// Auto-detect child access method.
        /// </summary>
        public static int Count<TreeT, NodeT>(TreeT tree, NodeT root)
        {
            CountLeaves<TreeT, NodeT, IteratorT> counter = new CountLeaves<TreeT, NodeT, IteratorT>();
            return counter.Count(tree, root);
        }

        /// <summary>
        /// Count leaves.
        /// Auto-detect child access methods.
        /// </summary>
        public static int Count<NodeT>(NodeT root)
        {
            return Count<NodeT, NodeT>(root, root);
        }


        /// <summary>
        /// Count leaves.
        /// </summary>
        public static int Count<TreeT, NodeT>(TreeT tree, NodeT root,
            TreeDefs<TreeT, NodeT, IteratorT>.GetChildDelegate getChild)
        {
            CountLeaves<TreeT, NodeT, IteratorT> counter = new CountLeaves<TreeT, NodeT, IteratorT>
            {
                GetChild = getChild,
            };
            return counter.Count(tree, root);
        }

        /// <summary>
        /// Count leaves.
        /// </summary>
        public static int Count<NodeT>(NodeT root, TreeDefs<NodeT, NodeT, IteratorT>.GetChildDelegate getChild)
        {
            return Count<NodeT, NodeT>(root, root, getChild);
        }

    }
}
