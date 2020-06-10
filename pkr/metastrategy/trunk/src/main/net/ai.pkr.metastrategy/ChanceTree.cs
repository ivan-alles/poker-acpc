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
    public unsafe class ChanceTree : UFTree
    {
        public ChanceTree()
        {
        }

        public ChanceTree(Int64 nodesCount)
            : base(nodesCount, Marshal.SizeOf(typeof(ChanceTreeNode)))
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
                _nodes = (ChanceTreeNode*) _nodesPtr.Ptr.ToPointer();
            }
        }

        public unsafe ChanceTreeNode * Nodes
        {
            get { return _nodes; }
        }

        public int PlayersCount
        {
            get { return Nodes[0].Position; }
            set { Nodes[0].Position = value; }
        }

        /// <summary>
        /// Calculates number of rounds by traversing the tree to the maximal depth along one path.
        /// </summary>
        /// <peturns></peturns>
        public int CalculateRoundsCount()
        {
            int playerCount = Nodes[0].Position;
            byte chanceDepth = 0;
            for (Int64 n = 1; n < NodesCount; ++n)
            {
                if (GetDepth(n) < chanceDepth)
                {
                    break;
                }
                chanceDepth = GetDepth(n);
            }
            if (chanceDepth % playerCount != 0)
            {
                throw new ApplicationException("Inconsistent data");
            }
            int roundsCount = chanceDepth / playerCount;
            return roundsCount;
        }

        ChanceTreeNode* _nodes;
    }
}
