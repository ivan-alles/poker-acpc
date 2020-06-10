/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Copies a Uni tree to Uf tree or vice versa. May also be used to compare such trees.
    /// <para>Input: a Uni tree and a Uf tree with the same number of nodes. The depths of the Uf tree may be uninitialized, 
    /// because the structure will be taken from the UniTree.
    /// </para>
    /// <para>The algo traverses each node of the Uni tree in preorder and takes the corresponding node of the Uf tree. 
    /// It calls the user-defined function that should copy/compare the data and/or set the depth of the UfTree.
    /// </para>
    /// </summary>
    public static class SyncUniAndUF<IteratorT>
    {
        /// <summary>
        /// Is called for each pair of nodes.
        /// </summary>
        /// <param name="userData">Can be used to pass some additional info to/from the algo.</param>
        /// <returns>If returns false, the algo stops and returns false. This may be used to compare the trees.</returns>
        public delegate bool SyncDelegate<TreeT, NodeT>(TreeT uniTree, NodeT uniNode, byte uniDepth, IteratorT uniIt, UFTree ufTree, Int64 ufNode, object userData);

        /// <summary>
        /// Syncronize trees.
        /// </summary>
        /// <param name="userData">User data to pass from/to the algo.</param>
        /// <returns></returns>
        public static bool Sync<TreeT, NodeT>(TreeT uniTree, NodeT uniRoot, UFTree ufTree, SyncDelegate<TreeT, NodeT> sync, object userData)
        {
            Data<TreeT, NodeT> data = new Data<TreeT, NodeT>
            {
                Sync = sync,
                UniGetChild = TreeDefs<TreeT, NodeT, IteratorT>.FindTreeGetChildMethod(),
                UniTree = uniTree,
                UfTree = ufTree,
                UfNode = 0,
                UserData = userData
            };

            return SyncInternal(data, uniRoot, 0, default(IteratorT));
        }

        /// <summary>
        /// Synchronize trees.
        /// </summary>
        public static bool Sync<TreeT, NodeT>(TreeT uniTree, NodeT uniRoot, UFTree ufTree, SyncDelegate<TreeT, NodeT> sync)
        {
            return Sync(uniTree, uniRoot, ufTree, sync, null);
        }


        /// <summary>
        /// Synchronize trees.
        /// </summary>
        public static bool Sync<NodeT>(NodeT uniRoot, UFTree ufTree, SyncDelegate<NodeT, NodeT> sync, object userData)
        {
            return Sync(uniRoot, uniRoot, ufTree, sync, userData);
        }

        /// <summary>
        /// Synchronize trees.
        /// </summary>
        public static bool Sync<NodeT>(NodeT uniRoot, UFTree ufTree, SyncDelegate<NodeT, NodeT> sync)
        {
            return Sync(uniRoot, uniRoot, ufTree, sync, null);
        }


        /// <summary>
        /// Global data.
        /// </summary>
        class Data<TreeT, NodeT>
        {
            public TreeT UniTree;
            public TreeDefs<TreeT, NodeT, IteratorT>.GetChildDelegate UniGetChild;
            public UFTree UfTree;
            public Int64 UfNode;
            public SyncDelegate<TreeT, NodeT> Sync;
            public object UserData;
        }

        static bool SyncInternal<TreeT, NodeT>(Data<TreeT, NodeT> data, NodeT uniNode, byte depth, IteratorT uniIt)
        {
            if(!data.Sync(data.UniTree, uniNode, depth, uniIt, data.UfTree, data.UfNode, data.UserData))
            {
                return false;
            }
            data.UfNode++;
            NodeT uniChild;
            IteratorT it = default(IteratorT);
            while (data.UniGetChild(data.UniTree, uniNode, ref it, out uniChild))
            {
                if (!SyncInternal(data, uniChild, (byte)(depth + 1), it))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
