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
    /// Context for WalkUFTreePP.
    /// </summary>
    public class WalkUFTreePPContext
    {
        /// <summary>
        /// Index of the UF tree node.
        /// </summary>
        public Int64 NodeIdx;
        /// <summary>
        /// Number of children seen so far. Is updated before OnNodeBegin() for each child. 
        /// In OnNodeEnd() is the total number of children,
        /// leaves will have 0.
        /// </summary>
        public int ChildrenCount;
    }

    /// <summary>
    /// Walks a flat tree tree in both pre-order (OnNodeBegin()) and post-order (OnNodeEnd()).
    /// <para>It can be customized in two ways:</para>
    /// <para>1. By overriding virtual functions.</para>
    /// <para>2. By setting delegate properies (this has priority over virtual functions)</para>
    /// </summary>
    /// Developer notes:
    /// 1. There is not such a feature as stop walking (compare to WalkTreePP). If this is necessary, just throw an exception.
    public class WalkUFTreePP<TreeT, ContextT>
        where TreeT: UFTree
        where ContextT : WalkUFTreePPContext, new()
    {
        #region Public types

        public delegate void OnTreeBeginDelegate(TreeT tree);
        public delegate void OnTreeEndDelegate(TreeT tree);

        public delegate void OnNodeBeginDelegate(TreeT tree, ContextT[] stack, int depth);
        public delegate void OnNodeEndDelegate(TreeT tree, ContextT[] stack, int depth);


        #endregion

        #region Public Interface

        public WalkUFTreePP()
        {
            OnTreeBegin = OnTreeBeginFunc;
            OnTreeEnd = OnTreeEndFunc;
            OnNodeBegin = OnNodeBeginFunc;
            OnNodeEnd = OnNodeEndFunc;
        }

        /// <summary>
        /// A delegate called on tree begin.
        /// </summary>
        public OnTreeBeginDelegate OnTreeBegin
        {
            set
            {
                if (value == null)
                {
                    _onTreeBegin = OnTreeBeginFunc;
                }
                else
                {
                    _onTreeBegin = value;

                }
            }
            get { return _onTreeBegin; }
        }

        /// <summary>
        /// A delegate called on tree end.
        /// </summary>
        public OnTreeEndDelegate OnTreeEnd
        {
            set
            {
                if (value == null)
                {
                    _onTreeEnd = OnTreeEndFunc;
                }
                else
                {
                    _onTreeEnd = value;

                }
            }
            get { return _onTreeEnd; }
        }

        /// <summary>
        /// A delegate called on node first seen, before the children are processed.
        /// If returns false, no further processing of this node is done 
        /// (no calls of GetChild, OnNodeEnd).
        /// </summary>
        public OnNodeBeginDelegate OnNodeBegin
        {
            set
            {
                if (value == null)
                {
                    _onNodeBegin = OnNodeBeginFunc;
                }
                else
                {
                    _onNodeBegin = value;

                }
            }
            get { return _onNodeBegin; }
        }

        /// <summary>
        /// A delegate called after all children have been processed.
        /// </summary>
        public OnNodeEndDelegate OnNodeEnd
        {
            set
            {
                if(value == null)
                {
                    _onNodeEnd = OnNodeEndFunc;
                }
                else
                {
                    _onNodeEnd = value;

                }
            }
            get { return _onNodeEnd;  }
        }


        private const int DEFAULT_DEPTH_LIMIT = 256;

        /// <summary>
        /// Walks the whole tree.
        /// </summary>
        public virtual void Walk(TreeT tree)
        {
            Walk(tree, 0);
        }

        /// <summary>
        /// Walks the tree from the given node
        /// </summary>
        public virtual void Walk(TreeT tree, Int64 startNode)
        {
            _onTreeBegin(tree);
            
            ContextT[] stack = new ContextT[DEFAULT_DEPTH_LIMIT].Fill(i => new ContextT());
            Int32 startDepth = tree.GetDepth(startNode);
            Int32 depth = -1;
            int curDepth;
            for (Int64 i = startNode; i < tree.NodesCount; ++i)
            {
                curDepth = tree.GetDepth(i);
                // Check depth first because it is almost always false, 
                // and the second check will be not necessary.
                if (curDepth <= startDepth && i > startNode)
                {
                    break;
                }
                for (; depth >= curDepth; --depth)
                {
                    _onNodeEnd(tree, stack, depth);
                }
                depth = curDepth;
                stack[depth].NodeIdx = i;
                stack[depth].ChildrenCount = 0;
                if (depth > 0)
                {
                    stack[depth - 1].ChildrenCount++;
                }
                _onNodeBegin(tree, stack, depth);
            }
            curDepth = startDepth;
            for (; depth >= curDepth; --depth)
            {
                _onNodeEnd(tree, stack, depth);
            }
            _onTreeEnd(tree);
        }


        #endregion

        #region Protected members

        protected virtual void OnTreeBeginFunc(TreeT tree)
        {
        }

        protected virtual void OnTreeEndFunc(TreeT tree)
        {
        }

        /// <summary>
        /// Is called when the node is first visited, before processing children.
        /// </summary>
        /// <returns>false to skip processing children of the node.</returns>
        protected virtual void OnNodeBeginFunc(TreeT tree, ContextT[] stack, int depth)
        {
        }

        protected virtual void OnNodeEndFunc(TreeT tree, ContextT[] stack, int depth)
        {
        }
        
        #endregion

        #region Implementation

        private OnTreeBeginDelegate _onTreeBegin;
        private OnTreeEndDelegate _onTreeEnd;
        private OnNodeBeginDelegate _onNodeBegin;
        private OnNodeEndDelegate _onNodeEnd;

        #endregion
        
    }

}
