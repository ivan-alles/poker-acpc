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
using ai.pkr.metastrategy;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Non-generic definitions.
    /// </summary>
    public class WalkChanceAndActionTrees
    {
        /// <summary>
        /// Inidcates current tree kind, can be used as 
        /// index for to access trees or related data in an array.
        /// </summary>
        public enum TreeKind
        {
            Action = 0,
            Chance = 1,
            _Count
        }
    }

    public class WalkChanceAndActionTreesContext
    {
        public WalkChanceAndActionTrees.TreeKind TreeKind;
        public Int64[] NodeIdx;
    }

    /// <summary>
    /// Walks chance and action trees, switching between them in nodes where dealer and players switch.
    /// The resulting order structure corresponds more or less to the strategy tree.
    /// The roots of both trees are ignores.
    /// To use this algo, derive a class and override OnNodeBegin().
    /// </summary>
    public unsafe class WalkChanceAndActionTrees<ContextT> : WalkChanceAndActionTrees
        where ContextT : WalkChanceAndActionTreesContext, new()
    {

        #region Public Interface

        public WalkChanceAndActionTrees()
        {
        }

        public ActionTree ActionTree
        {
            set;
            get;
        }

        public ChanceTree PlayerChanceTree
        {
            set;
            get;
        }

        protected UFTree[] Trees
        {
            private set;
            get;
        }

        /// <summary>
        /// Walks the tree.
        /// </summary>
        public void Walk()
        {
            Trees = new UFTree[] { ActionTree, PlayerChanceTree};

            _indexes = new Dictionary<Int64, List<Int64>>[2].Fill( i => new Dictionary<Int64, List<Int64>>());


            WalkUFTreePP<ChanceTree, Context> wtc = new WalkUFTreePP<ChanceTree, Context>();
            wtc.OnNodeBegin = OnNodeBeginChance;
            wtc.Walk(PlayerChanceTree);

            WalkUFTreePP<ActionTree, Context> wta = new WalkUFTreePP<ActionTree, Context>();
            wta.OnNodeBegin = OnNodeBeginAction;
            wta.Walk(ActionTree);

            ContextT[] stack = new ContextT[DEFAULT_DEPTH_LIMIT].Fill(i => new ContextT());
            WalkRecursive(TreeKind.Action, new Int64[] { 0, 0 }, stack, -1);
        }

        #endregion

        #region Protected overridables

        /// <summary>
        /// Is called when the node is first visited, before processing children.
        /// </summary>
        /// <returns>false to skip processing children of the node.</returns>
        protected virtual void OnNodeBegin(ContextT[] stack, int depth)
        {
        }

        #endregion

        #region Implementation

        class Context : WalkUFTreePPContext
        {
            public Int64 ParentIdx = 0;
        }

        void OnNodeBeginChance(ChanceTree tree, Context[] stack, int depth)
        {
            if (depth > 0)
            {
                // Include all children to the list of children of the parent.
                Int64 parentNodeIdx = stack[depth-1].NodeIdx;
                List<Int64> children;
                if (!_indexes[(int)TreeKind.Chance].TryGetValue(parentNodeIdx, out children))
                {
                    children = new List<Int64>();
                    _indexes[(int)TreeKind.Chance].Add(parentNodeIdx, children);
                }
                children.Add(stack[depth].NodeIdx);
            }
        }

        void OnNodeBeginAction(ActionTree tree, Context[] stack, int depth)
        {
            if (depth > 0)
            {
                stack[depth].ParentIdx = stack[depth - 1].ParentIdx;
                // Look for nodes where the round changes.
                if (tree.Nodes[stack[depth-1].NodeIdx].Round < tree.Nodes[stack[depth].NodeIdx].Round)
                {
                    stack[depth].ParentIdx = stack[depth-1].NodeIdx;
                }
                Int64 parentNodeIdx = stack[depth].ParentIdx;
                List<Int64> children;
                if (!_indexes[(int) TreeKind.Action].TryGetValue(parentNodeIdx, out children))
                {
                    children = new List<Int64>();
                    _indexes[(int)TreeKind.Action].Add(parentNodeIdx, children);
                }
                children.Add(stack[depth].NodeIdx);
            }
        }

        /// <summary>
        /// Walks the tree.
        /// </summary>
        public void WalkRecursive(TreeKind treeKind, Int64[] nodeIdx, ContextT[] stack, int globalDepth)
        {
            UFTree tree = Trees[(int)treeKind];
            Int64 parentIdx = nodeIdx[(int)treeKind];
            int parentDepth = tree.GetDepth(parentIdx);

            List<Int64> children = _indexes[(int)treeKind][parentIdx];

            for (int c = 0; c < children.Count; ++c)
            {
                Int64 childIdx = children[c];
                int childDepth = tree.GetDepth(childIdx);
                int childGlobalDepth = globalDepth + childDepth - parentDepth;

                stack[childGlobalDepth].TreeKind = treeKind;
                stack[childGlobalDepth].NodeIdx = CreateNodeIdx(nodeIdx, treeKind, childIdx);
                OnNodeBegin(stack, childGlobalDepth);

                bool switchTree = treeKind == TreeKind.Chance ? true : _indexes[(int)treeKind].ContainsKey(childIdx);
                if (switchTree)
                {
                    WalkRecursive(ReverseTreeKind(treeKind), CreateNodeIdx(nodeIdx, treeKind, childIdx), stack, childGlobalDepth);
                }
            }
        }

        TreeKind ReverseTreeKind(TreeKind treeKind)
        {
            return (TreeKind)(1 - (int)treeKind);
        }

        Int64[] CreateNodeIdx(Int64[] initialNodeIdx, TreeKind treeKind, Int64 curNodeIdx)
        {
            Int64[] result = new Int64[] { initialNodeIdx[0], initialNodeIdx[1] };
            result[(int)treeKind] = curNodeIdx;
            return result;
        }

        /// <summary>
        /// For each tree kind: mapping between each node where 
        /// the tree is switched and its children.
        /// </summary>
        Dictionary<Int64, List<Int64>>[] _indexes;

        private const int DEFAULT_DEPTH_LIMIT = 255;

        #endregion
    }

}
