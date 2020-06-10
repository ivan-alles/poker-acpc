/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using System.IO;
using System.Runtime.InteropServices;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Action tree. Blinds are defined by player actions in round -1.
    /// </summary>
    public unsafe class ActionTree : UFTree
    {
        public ActionTree()
        {
        }

        public ActionTree(Int64 nodesCount)
            : base(nodesCount, Marshal.SizeOf(typeof(ActionTreeNode)))
        {
            //UnmanagedMemory.SetMemory(_nodesPtr.Ptr, _nodesByteSize, 0);
            AfterRead();
        }

        protected override void WriteUserData(BinaryWriter w)
        {
        }

        protected override void ReadUserData(BinaryReader r)
        {
        }

        protected override void AfterRead()
        {
            if (_nodesPtr != null)
            {
                _nodes = (ActionTreeNode*) _nodesPtr.Ptr.ToPointer();
            }
        }

        public unsafe ActionTreeNode * Nodes
        {
            get { return _nodes; }
        }

        public int PlayersCount
        {
            get { return Nodes[0].Position; }
            set { Nodes[0].Position = value; }
        }

        /// <summary>
        /// Finds a child of node with the given amount by looking up all children using the index..
        /// </summary>
        public long FindChildByAmount(long nodeIdx, double amount, UFTreeChildrenIndex index)
        {
            int childCount, chBegin;
            index.GetChildrenBeginIdxAndCount(nodeIdx, out chBegin, out childCount);
            for (int ch = 0; ch < childCount; ++ch)
            {
                int chIdx = index.GetChildIdx(chBegin + ch);
                if (Nodes[chIdx].Amount == amount)
                {
                    return chIdx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds a child of node with the given amount. A child pointed by hint is checked first.
        /// This allows to skip searching if the position of this child is known (for example from another 
        /// tree for the same game).
        /// </summary>
        public long FindChildByAmount(long nodeIdx, double amount, UFTreeChildrenIndex index, int hintChildIdx)
        {
            int chBegin, chCount;
            index.GetChildrenBeginIdxAndCount(nodeIdx, out chBegin, out chCount);
            if (hintChildIdx < chCount)
            {
                int chIdx = index.GetChildIdx(chBegin + hintChildIdx);
                if (Nodes[chIdx].Amount == amount)
                {
                    return chIdx;
                }
            }
            return FindChildByAmount(nodeIdx, amount, index);
        }



        ActionTreeNode* _nodes;
    }
}
