/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// A class that walks a tree and searches for a node that matches a given
    /// predicate. Matching is done before iterating the children.
    /// </summary>
    public class FindFirstPreOrder<TreeT, NodeT, IteratorT> : WalkTreePP<TreeT, NodeT, IteratorT>
    {
        #region Public
        public Predicate<NodeT> Match
        {
            set;
            get;
        }

        /// <summary>
        /// If found, contains the node, otherwise default(NodeT).
        /// </summary>
        public NodeT Result
        {
            get;
            protected set;
        }

        /// <summary>
        /// Search the tree.
        /// </summary>
        public bool Find(TreeT tree, NodeT root)
        {
            Walk(tree, root);
            return _found;
        }

        #endregion

        #region Implementation

        protected override void OnTreeBeginFunc(TreeT tree, NodeT root)
        {
            Result = default(NodeT);
            _found = false;            
        }

        protected override bool OnNodeBeginFunc(TreeT tree, NodeT node, List<WalkTreePPContext<NodeT, IteratorT>> stack, int depth)
        {
            if(Match(node))
            {
                Result = node;
                _found = true;
                Terminate = true;
                return false;
            }
            return true;
        }

        bool _found = false;

        #endregion
    }

    /// <summary>
    /// Contains static helper functions that use FindFirstPreOrder&lt;...&gt;.
    /// They are easier to use, but they are slower in repeated searches 
    /// than creating and using an instance of FindFirstPreOrder&lt;&gt;.
    /// </summary>
    /// <typeparam name="IteratorT">Tree iterator type (it cannot be autodetected by the compiler).</typeparam>
    public class FindFirstPreOrder<IteratorT>
    {
        /// <summary>
        /// Searches the tree. If found, returns the node, otherwise default(NodeT).
        /// Auto-detect child access method.
        /// </summary>
        public static NodeT Find<TreeT, NodeT>(TreeT tree, NodeT root, Predicate<NodeT> match)
        {
            FindFirstPreOrder<TreeT, NodeT, IteratorT> finder = new FindFirstPreOrder<TreeT, NodeT, IteratorT>()
            {
                Match = match
            };
            finder.Find(tree, root);
            return finder.Result;
        }

        /// <summary>
        /// Searches the tree. If found, returns the node, otherwise default(NodeT).
        /// Auto-detect child access methods.
        /// </summary>
        public static NodeT Find<NodeT>(NodeT root, Predicate<NodeT> match)
        {
            return Find<NodeT, NodeT>(root, root, match);
        }


        /// <summary>
        /// Searches the tree. If found, returns the node, otherwise default(NodeT).
        /// </summary>
        public static NodeT Find<TreeT, NodeT>(TreeT tree, NodeT root, Predicate<NodeT> match,
            TreeDefs<TreeT, NodeT, IteratorT>.GetChildDelegate getChild)
        {
            FindFirstPreOrder<TreeT, NodeT, IteratorT> finder = new FindFirstPreOrder<TreeT, NodeT, IteratorT>
            {
                GetChild = getChild,
                Match = match
            };
            finder.Find(tree, root);
            return finder.Result;
        }

        /// <summary>
        /// Searches the tree. If found, returns the node, otherwise default(NodeT).
        /// </summary>
        public static NodeT Find<NodeT>(NodeT root, Predicate<NodeT> match,
            TreeDefs<NodeT, NodeT, IteratorT>.GetChildDelegate getChild)
        {
            return Find<NodeT, NodeT>(root, root, match, getChild);
        }

    }
}
