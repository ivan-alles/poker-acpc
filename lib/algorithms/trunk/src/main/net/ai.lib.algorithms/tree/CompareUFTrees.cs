/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace ai.lib.algorithms.tree
{

    /// <summary>
    /// Non-generic definitions for CompareUFTrees&lt;&gt; for internal purposes.
    /// Use non-generic CompareUFTrees instead.
    /// </summary> 
    /// Developer notes:
    /// It is an interface to allow (in future) multiple inheritance without breaking changes.
    public class ICompareUFTreesDefs
    {
        public enum ResultKind
        {
            Equal,
            StructureDiffers,
            ValueDiffers
        }
    }

    /// <summary>
    /// Non-generic definitions for CompareUFTrees&lt;&gt;. 
    /// </summary> 
    /// Developer notes:
    /// This is better to use in code as ICompareUFTreesDefs.
    public class CompareUFTrees : ICompareUFTreesDefs
    {
    }

    /// <summary>
    /// Comparer for 2 trees. The trees can be of different types. 
    /// It compares both tree structure and node values.
    /// <para>It can also be used in a broader sense, to calculate a difference of values between two trees
    /// with the same structure.
    /// In this case pass a compare delegate that always returns true, and use it to compute the difference.</para>
    /// </summary>
    public class CompareUFTrees<TreeT1, TreeT2> : ICompareUFTreesDefs where TreeT1: UFTree where TreeT2: UFTree
    {
        #region Public members

        /// <summary>
        /// A delegate to compare nodes. Must return true if and only if the nodes are equal.
        /// Example: (t1, t2, n) => t1.Nodes[n].Value == t2.Nodes[n].Value
        /// </summary>
        public delegate bool CompareDelegate(TreeT1 tree1, TreeT2 tree2, Int64 n);

        /// <summary>
        /// Result of the comparison.
        /// </summary>
        public ResultKind Result
        {
            protected set;
            get;
        }

        /// <summary>
        /// For unequal trees: 
        /// <para>- if the number of nodes is the same: the first node in preorder traversal where the trees differ.</para>
        /// <para>- if the number of nodes differs: -1</para>
        /// </summary>
        public Int64 DiffersAt
        {
            protected set;
            get;
        }

        /// <summary>
        /// Compares trees.
        /// </summary>
        public bool Compare(TreeT1 tree1, TreeT2 tree2, CompareDelegate compare)
        {
            if (tree1.NodesCount != tree2.NodesCount)
            {
                DiffersAt = -1;
                Result = ResultKind.StructureDiffers;
                return false;
            }

            Int32 depth = -1;
            for (Int64 i = 0; i < tree1.NodesCount; ++i)
            {
                depth = tree1.GetDepth(i);
                if (depth != tree2.GetDepth(i))
                {
                    DiffersAt = i;
                    Result = ResultKind.StructureDiffers;
                    return false;
                }
                if(!compare(tree1, tree2, i))
                {
                    DiffersAt = i;
                    Result = ResultKind.ValueDiffers;
                    return false;
                }
            }
            DiffersAt = -1;
            Result = ResultKind.Equal;
            return true;
        }

        /// <summary>
        /// Compares trees.
        /// </summary>
        public static bool CompareS(TreeT1 tree1, TreeT2 tree2, CompareDelegate compare)
        {
            CompareUFTrees<TreeT1, TreeT2> comparer = new CompareUFTrees<TreeT1, TreeT2>();
            return comparer.Compare(tree1, tree2, compare);
        }


        #endregion
    }
}
