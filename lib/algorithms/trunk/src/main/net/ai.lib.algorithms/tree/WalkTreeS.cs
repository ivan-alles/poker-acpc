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
    /// Base class for WalkTreeS context.
    /// </summary>
    public class WalkTreeSContext<NodeT, IteratorT>
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

        /// <summary>
        /// List of children.
        /// </summary>
        internal List<NodeT> Children = new List<NodeT>();
        /// <summary>
        /// Index of current children in Children list.
        /// </summary>
        internal int CurrentChildInList;
    }

    /// <summary>
    /// Walks a tree in siblings-order, as illustrated in design\walk-tree-siblings-order.gv.
    /// <para>It can be customized in two ways:</para>
    /// <para>1. By overriding virtual functions.</para>
    /// <para>2. By setting delegate properies (this has priority over virtual functions)</para>
    /// 
    /// <para>It is necessary to tell this algorithm how to access children of a tree node.</para>
    /// <para>To do it, use one of the following:</para>
    /// <para>1. Set GetChild property</para>
    /// <para>2. Create static method TreeGetChild in the TreeT class.</para>
    /// <para>Take into account that the algorithm stores all children of each node in the stack in a list. 
    /// If the branching factor is large, it can lead to long lists of children, 
    /// they live until the node is removed from the stack.</para>
    /// </summary>
    public class WalkTreeS<TreeT, NodeT, IteratorT, ContextT> where ContextT : WalkTreeSContext<NodeT, IteratorT>, new()
    {
        #region Public types

        public delegate void OnTreeBeginDelegate(TreeT tree, NodeT root);
        public delegate void OnTreeEndDelegate(TreeT tree, NodeT root);

        public delegate bool OnNodeBeginDelegate(TreeT tree, NodeT child, List<ContextT> stack, int depth);
        public delegate void OnNodeEndDelegate(TreeT tree, NodeT node, List<ContextT> stack, int depth);
        public delegate void ParentDelegate(TreeT tree, NodeT node, List<ContextT> stack, int depth);

        #endregion

        #region Public Interface

        public WalkTreeS()
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
        /// <para>Remember, that in this function the node is not put on the stack yet! Therefore depth is
        /// one less that the actual depth, and you cannot access the context of this node.
        /// </para>
        /// </summary>
        public OnNodeBeginDelegate OnNodeBegin
        {
            set;
            get;
        }

        /// <summary>
        /// A delegate called for a node before processising the direct children..
        /// </summary>
        public ParentDelegate BeforeDirectChildren
        {
            set;
            get;
        }


        /// <summary>
        /// A delegate called for a node before processising the direct children..
        /// </summary>
        public ParentDelegate AfterDirectChildren
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
            // The list of children is cleared every time the node is removed from the stack,
            // therefore it either new or cleared here.
            stack[0].CurrentChildInList = 0;

            if ((PruneIf == null || !PruneIf(root)) && OnNodeBegin(tree, root, stack, depth-1))
            {

                while (!Terminate)
                {
                    ContextT parentContext = stack[depth];
                    ContextT childContext;

                    if (parentContext.CurrentChildInList == 0)
                    {
                        // We have not processed the children yet.

                        BeforeDirectChildren(tree, parentContext.Node, stack, depth);

                        NodeT child;

                        while (GetChild(tree, parentContext.Node, ref parentContext.ChildrenIt, out child))
                        {
                            if ((PruneIf == null || !PruneIf(child)) && OnNodeBegin(tree, child, stack, depth))
                            {
                                parentContext.Children.Add(child);
                            }
                            if(Terminate)
                            {
                                break;
                            }
                        }

                        AfterDirectChildren(tree, parentContext.Node, stack, depth);
                    }

                    // Now put the children from the list on the stack one by one and process them recursively.
                    if (parentContext.CurrentChildInList < parentContext.Children.Count)
                    {
                        depth++;
                        if (stack.Count == depth)
                        {
                            stack.Add(new ContextT());
                        }
                        childContext = stack[depth];
                        childContext.Node = parentContext.Children[parentContext.CurrentChildInList++];
                        // The list of children is cleared every time the node is removed from the stack,
                        // therefore it either new or cleared here.
                        childContext.CurrentChildInList = 0;
                        childContext.ChildrenIt = default(IteratorT);
                    }
                    else
                    {
                        // No more children.
                        OnNodeEnd(tree, parentContext.Node, stack, depth);
                        // Release children.
                        parentContext.Children.Clear();
                        parentContext.Node = default(NodeT);
                        if (--depth < 0)
                            break;
                    }
                }
            }
            stack[0].Children.Clear();
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

        protected virtual void BeforeDirectChildrenFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
        }

        protected virtual void AfterDirectChildrenFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
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
            BeforeDirectChildren = BeforeDirectChildrenFunc;
            AfterDirectChildren = AfterDirectChildrenFunc;
        }

        #endregion
    }

    /// <summary>
    /// A shortcut for WalkTreeS&lt;...&gt; that uses
    /// the standard type for the context.
    /// </summary>
    /// <typeparam name="NodeT">tree node type</typeparam>
    public class WalkTreeS<TreeT, NodeT, IteratorT> : WalkTreeS<TreeT, NodeT, IteratorT, WalkTreeSContext<NodeT, IteratorT>>
    {
    }

    /// <summary>
    /// A shortcut for WalkTreeSe&lt;...&gt; for most typical usecase 
    /// where TreeT == NodeT and standard context is used.
    /// </summary>
    public class WalkTreeS<NodeT, IteratorT> : WalkTreeS<NodeT, NodeT, IteratorT, WalkTreeSContext<NodeT, IteratorT>>
    {
    }
}
