/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Base class for WalkTreePP context.
    /// </summary>
    public class WalkTreePPContext<NodeT, IteratorT>
    {
        /// <summary>
        /// Current tree node.
        /// </summary>
        public NodeT Node
        {
            set;
            get;
        }

        public override string ToString()
        {
            if (Node == null)
                return "null";
            return Node.ToString();
        }

        /// <summary>
        /// Children iterator, in OnNode* callback points to the next child.
        /// </summary>
        public IteratorT ChildrenIterator
        {
            get { return ChildrenIt; }
        }

        internal IteratorT ChildrenIt;
    }

    /// <summary>
    /// Walks a tree in both pre-order (OnNodeBegin()) and post-order (OnNodeEnd()).
    /// <para>It can be customized in two ways:</para>
    /// <para>1. By overriding virtual functions.</para>
    /// <para>2. By setting delegate properies (this has priority over virtual functions)</para>
    /// 
    /// <para>It is necessary to tell this algorithm how to access children of a tree node.</para>
    /// <para>To do it, use one of the following:</para>
    /// <para>1. Set GetChild property</para>
    /// <para>2. Create static method TreeGetChild in the TreeT class.</para>
    /// </summary>
    public class WalkTreePP<TreeT, NodeT, IteratorT, ContextT> where ContextT : WalkTreePPContext<NodeT, IteratorT>, new()
    {
        #region Public types

        public delegate void OnTreeBeginDelegate(TreeT tree, NodeT root);
        public delegate void OnTreeEndDelegate(TreeT tree, NodeT root);

        public delegate bool OnNodeBeginDelegate(TreeT tree, NodeT node, List<ContextT> stack, int depth);
        public delegate void OnNodeEndDelegate(TreeT tree, NodeT node, List<ContextT> stack, int depth);


        #endregion

        #region Public Interface

        public WalkTreePP()
        {
            Initialize();
        }

        /// <summary>
        /// A delegate returning a child.
        /// </summary>
        public TreeDefs<TreeT, NodeT, IteratorT>.GetChildDelegate GetChild
        {
            set;
            get;
        }

        /// <summary>
        /// A delegate called on tree begin.
        /// </summary>
        public OnTreeBeginDelegate OnTreeBegin
        {
            set;
            get;
        }

        /// <summary>
        /// A delegate called on tree end.
        /// </summary>
        public OnTreeEndDelegate OnTreeEnd
        {
            set;
            get;
        }

        /// <summary>
        /// A delegate called on node first seen, before the children are processed.
        /// If returns false, no further processing of this node is done 
        /// (no calls of GetChild, OnNodeEnd).
        /// </summary>
        public OnNodeBeginDelegate OnNodeBegin
        {
            set;
            get;
        }

        /// <summary>
        /// A delegate called after all children have been processed.
        /// </summary>
        public OnNodeEndDelegate OnNodeEnd
        {
            set;
            get;
        }

        /// <summary>
        /// Defines a predicate that controls if some subtrees will be pruned.
        /// If it returns true on a node, the subtree with root at this node will not
        /// be walked. This is similar to returning false from OnNodeBegin,
        /// but allows to prune without modifying OnNodeBegin.
        /// The argument of the predicate is the node, and not the context, because the 
        /// context is not initialized yet anyway.
        /// </summary>
        public Predicate<NodeT> PruneIf
        {
            set;
            get;
        }

        /// <summary>
        /// Set to true to stop further processing of nodes immediately and exit.
        /// Can be set in OnTreeBegin(), OnNodeBegin() or OnNodeEnd() methods.
        /// OnTreeEnd() will be called.
        /// </summary>
        public bool Terminate
        {
            set;
            get;
        }

        /// <summary>
        /// Walks the tree.
        /// </summary>
        public virtual void Walk(TreeT tree, NodeT root)
        {
            if (GetChild == null)
            {
                GetChild = TreeDefs<TreeT, NodeT, IteratorT>.FindTreeGetChildMethod();
            }

            Terminate = false;

            OnTreeBegin(tree, root);

            // Algorithm:
            // 1. Put root to the stack.
            // 2. If stack is empty, exit; othewise take a node from the stack. 
            // 3. If no pre-process have been done, do it: (PruneIf, OnBegin). If prune  - remove from the stack, goto 2.
            // 4  Take child, put it on the stack, goto 2. In no more chilren, post-process the node, 
            //    remove node from the stack, goto 2.
            //
            // Context in the stack are reused (not removed from stack), only stack depth is changed and
            // reference to node is cleared. This speeds-up the algo by about factor 30.

            List<ContextT> stack = new List<ContextT>(100);
            stack.Add(new ContextT {Node = root});

            int depth = 0;
            ContextT context = stack[0];
            while(!Terminate)
            {
                // Do preprocessing
                if ((PruneIf != null && PruneIf(context.Node)) || !OnNodeBegin(tree, context.Node, stack, depth))
                {
                    context.Node = default(NodeT);
                    if (--depth < 0)
                        break;
                    context = stack[depth];
                }

            skipPreprocessing:

                if (Terminate)
                    break;

                NodeT child;
                if (GetChild(tree, context.Node, ref context.ChildrenIt, out child))
                {
                    // Child exists
                    depth++;
                    if (stack.Count == depth)
                    {
                        stack.Add(new ContextT());
                    }
                    context = stack[depth];
                    context.Node = child;
                    context.ChildrenIt = default(IteratorT);
                }
                else
                {
                    // No more children.
                    OnNodeEnd(tree, context.Node, stack, depth);
                    context.Node = default(NodeT);
                    if (--depth < 0)
                        break;
                    context = stack[depth];
                    goto skipPreprocessing;
                }
            }
            OnTreeEnd(tree, root);
        }

        /// <summary>
        /// For the case TreeT == NodeT and tree is not necessary, calls Walk(default(TreeT), root)
        /// </summary>
        public void Walk(NodeT root)
        {
            Walk(default(TreeT), root);
        }

        #endregion

        #region Protected members


        protected virtual void OnTreeBeginFunc(TreeT tree, NodeT root)
        {
        }

        protected virtual void OnTreeEndFunc(TreeT tree, NodeT root)
        {
        }

        /// <summary>
        /// Is called when the node is first visited, before processing children.
        /// </summary>
        /// <returns>false to skip processing children of the node.</returns>
        protected virtual bool OnNodeBeginFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            return true;
        }

        protected virtual void OnNodeEndFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
        }
        
        #endregion

        #region Implementation

        private void Initialize()
        {
            OnTreeBegin = OnTreeBeginFunc;
            OnTreeEnd = OnTreeEndFunc;
            OnNodeBegin = OnNodeBeginFunc;
            OnNodeEnd = OnNodeEndFunc;
        }

        #endregion
    }

    /// <summary>
    /// A shortcut for WalkTreePP&lt;...&gt; that uses
    /// the standard type for the context.
    /// </summary>
    /// <typeparam name="NodeT">tree node type</typeparam>
    public class WalkTreePP<TreeT, NodeT, IteratorT> : WalkTreePP<TreeT, NodeT, IteratorT, WalkTreePPContext<NodeT, IteratorT>>
    {
    }

    /// <summary>
    /// A shortcut for WalkTreePPe&lt;...&gt; for most typical usecase 
    /// where TreeT == NodeT and standard context is used.
    /// </summary>
    public class WalkTreePP<NodeT, IteratorT> : WalkTreePP<NodeT, NodeT, IteratorT, WalkTreePPContext<NodeT, IteratorT>>
    {
    }
}
