/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Counts nodes in a tree.
    /// </summary>
    public class CountNodes<TreeT, NodeT, IteratorT> : WalkTreePP<TreeT, NodeT, IteratorT>
    {
        #region Public

        /// <summary>
        /// After call to Walk() or Count() contains number of nodes.
        /// </summary>
        public int NodeCount
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
            return NodeCount;
        }

        #endregion

        #region Implementation

        protected override void OnTreeBeginFunc(TreeT tree, NodeT root)
        {
            NodeCount = 0;
        }

        protected override void OnNodeEndFunc(TreeT tree, NodeT node, List<WalkTreePPContext<NodeT, IteratorT>> stack, int depth)
        {
            NodeCount++;
        }


        #endregion
    }

    /// <summary>
    /// Contains static helper functions that use CountNodes&lt;...&gt;.
    /// They are easier to use, but they are slower in repeated searches 
    /// than creating and using an instance of CountNodes&lt;&gt;.
    /// Only iterator type is a generic parameter of the class,
    /// the other generic parameters can be deduced by the compiler.
    /// </summary>
    public class CountNodes<IteratorT> 
    {
        /// <summary>
        /// Count nodes.
        /// Auto-detect child access method.
        /// </summary>
        public static int Count<TreeT, NodeT>(TreeT tree, NodeT root)
        {
            CountNodes<TreeT, NodeT, IteratorT> counter = new CountNodes<TreeT, NodeT, IteratorT>();
            return counter.Count(tree, root);
        }

        /// <summary>
        /// Count nodes.
        /// Auto-detect child access methods.
        /// </summary>
        public static int Count<NodeT>(NodeT root)
        {
            return Count<NodeT, NodeT>(root, root);
        }


        /// <summary>
        /// Count nodes.
        /// </summary>
        public static int Count<TreeT, NodeT>(TreeT tree, NodeT root,
            TreeDefs<TreeT, NodeT, IteratorT>.GetChildDelegate getChild)
        {
            CountNodes<TreeT, NodeT, IteratorT> counter = new CountNodes<TreeT, NodeT, IteratorT>
            {
                GetChild = getChild,
            };
            return counter.Count(tree, root);
        }

        /// <summary>
        /// Count nodes.
        /// </summary>
        public static int Count<NodeT>(NodeT root, TreeDefs<NodeT, NodeT, IteratorT>.GetChildDelegate getChild)
        {
            return Count<NodeT, NodeT>(root, root, getChild);
        }

    }
}
