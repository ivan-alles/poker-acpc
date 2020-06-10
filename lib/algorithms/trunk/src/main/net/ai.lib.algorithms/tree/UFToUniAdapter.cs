/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// An adapter allowing to use flat tree with the family of algorithms using UniTreeDefs TreeGetChild() 
    /// access to children. It indexes the flat tree (requieres 2*N Int32) and simulates the behavior of
    /// UniTree with TreeT = this type, NodeT = int (index of the node in the flat tree), IteratorT = int.
    /// <para>Usage:</para>
    /// <para>1. Create the adapter:</para>
    /// <para>UFToUniTree&lt;TestNode, int&gt; adapter = new UFToUniTree&lt;TestNode, int&gt;(flatTree);</para>
    /// <para>2. Create your algo class:</para>
    /// <para>WalkTreePP&lt;UFToUniTree&lt;TestNode, int&gt;, int, int, AdapterContext&gt; algo = new WalkTreePP ...</para>
    /// <para>3. Pass the adapter to the algo class, specify 0 as root node:</para>
    /// <para>algo.Walk(adapter, 0);</para>
    /// </summary>
    public class UFToUniAdapter : UFTreeChildrenIndex
    {
        #region Public API
        public static bool TreeGetChild(UFToUniAdapter tree, int n, ref int i, out int child)
        {
            int childrenBeginIdx, childrenCount;
            tree.GetChildrenBeginIdxAndCount(n, out childrenBeginIdx, out childrenCount);   
            if(i < childrenCount)
            {
                child = tree.GetChildIdx(childrenBeginIdx + i);
                i++;
                return true;
            }
            child = 0;
            return false;
        }

        public UFTree UfTree
        {
            set;
            get;
        }

        /// <summary>
        /// Creates new instance and indexes the tree (requires 2*N steps) for memory access.
        /// </summary>
        /// <param name="ufTree"></param>
        public UFToUniAdapter(UFTree ufTree): base(ufTree)
        {
            UfTree = ufTree;
        }

        /// <summary>
        /// Creates new instance for file access.
        /// </summary>
        /// <param name="ufTree">UF tree</param>
        /// <param name="forceCreation">If true - always create a new index file, otherwise create only if no file exists.</param>
        public UFToUniAdapter(UFTree ufTree, string fileName, bool forceCreation)
            : base(ufTree, fileName, forceCreation)
        {
            UfTree = ufTree;
        }

        #endregion

        #region Implementation

        #endregion
    }
}
