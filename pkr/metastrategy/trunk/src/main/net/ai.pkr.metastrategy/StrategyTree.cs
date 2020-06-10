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
    /// Strategy tree. Contains dealer and player nodes, including blinds.
    /// </summary>
    /// Developer nodes:
    /// 1. Inclusion of blind nodes allows to verify that the strategy matches the blind structure of a game.
    public unsafe class StrategyTree : UFTree
    {
        public StrategyTree()
        {
        }

        public StrategyTree(Int64 nodesCount)
            : base(nodesCount, Marshal.SizeOf(typeof(StrategyTreeNode)))
        {
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
                _nodes = (StrategyTreeNode*) _nodesPtr.Ptr.ToPointer();
            }
        }

        public unsafe StrategyTreeNode * Nodes
        {
            get { return _nodes; }
        }

        public int PlayersCount
        {
            get { return Nodes[0].Position; }
            set { Nodes[0].Position = value; }
        }

        /// <summary>
        /// Finds a node by an action path. Action path must not contain the root node and the blinds.
        /// This function is not speed-optimized.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Int64 FindNode(IStrategicAction[] path)
        {
            UFTreeChildrenIndex actionTreeIndex = new UFTreeChildrenIndex(this);
            Int64 curNode = PlayersCount;

            for (int a = 0; a < path.Length; ++a)
            {
                int childCount, chBegin;
                actionTreeIndex.GetChildrenBeginIdxAndCount(curNode, out chBegin, out childCount);
                for (int ch = 0; ch < childCount; ++ch)
                {
                    int chIdx = actionTreeIndex.GetChildIdx(chBegin + ch);
                    if (path[a].Position == Nodes[chIdx].Position)
                    {
                        if (Nodes[chIdx].IsDealerAction)
                        {
                            IDealerAction da = (IDealerAction) path[a];
                            if (da.Card == Nodes[chIdx].Card)
                            {
                                curNode = chIdx;
                                goto found;
                            }
                        }
                        else
                        {
                            IPlayerAction pa = (IPlayerAction) path[a];
                            if (pa.Amount == Nodes[chIdx].Amount)
                            {
                                curNode = chIdx;
                                goto found;
                            }
                        }
                    }
                }
                throw new ApplicationException(String.Format("Cannot find child at node {0} for action {1}",
                                                             curNode, path[a]));
                found:
                ;
            }
            return curNode;
        }

        /// <summary>
        /// Finds a node by a textual action path. Action path must not contain the root node and the blinds.
        /// This function is not speed-optimized.
        /// </summary>
        /// <param name="parameters">See StrategicString.FromStrategicString()</param>
        public Int64 FindNode(string path, object parameters)
        {
            List<IStrategicAction> actions = StrategicString.FromStrategicString(path, parameters);
            return FindNode(actions.ToArray());
        }

        StrategyTreeNode* _nodes;
    }
}
