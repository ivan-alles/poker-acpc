/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.algorithms.tree
{

    /// <summary>
    /// Non-generic definitions for CompareTrees&lt;&gt; for internal purposes.
    /// Use non-generic CompareTrees instead.
    /// </summary> 
    /// Developer notes:
    /// It is an interface to allow (in future) multiple inheritance without breaking changes.
    public class ICompareTreesDefs
    {
        public enum ResultKind
        {
            Equal,
            StructureDiffers,
            ValueDiffers
        }
    }

    /// <summary>
    /// Non-generic definitions for CompareTrees&lt;&gt;. 
    /// </summary> 
    /// Developer notes:
    /// This is better to use in code as ICompareTreesDefs.
    public class CompareTrees : ICompareTreesDefs
    {
    }

    /// <summary>
    /// Comparer for 2 trees. The trees can be of different types. 
    /// It compares both tree structure and node values.
    /// <para>It can also be used in a broader sense, to calculate a difference of values between two trees
    /// with the same structure.
    /// In this case pass a compare delegate that always returns true, and use it to compute the difference.</para>
    /// </summary>
    public class CompareTrees<TreeT1, NodeT1, IteratorT1, TreeT2, NodeT2, IteratorT2> : ICompareTreesDefs
    {
        #region Public members

        /// <summary>
        /// A delegate to compare nodes. Must return true if and only if the nodes are equal.
        /// Example: (t1, n1, t2, n2) => n1.value == n2.value
        /// </summary>
        public delegate bool CompareDelegate(TreeT1 tree1, NodeT1 node1, TreeT2 tree2, NodeT2 node2);

        /// <summary>
        /// Child accessor for tree1. Is auto-detected If TreeT1 contains TreeGetChild() method.
        /// </summary>
        public TreeDefs<TreeT1, NodeT1, IteratorT1>.GetChildDelegate GetChild1
        {
            set;
            get;
        }

        /// <summary>
        /// Child accessor for tree2. Is auto-detected If TreeT2 contains TreeGetChild() method.
        /// </summary>
        public TreeDefs<TreeT2, NodeT2, IteratorT2>.GetChildDelegate GetChild2
        {
            set;
            get;
        }

        /// <summary>
        /// Result of the comparison.
        /// </summary>
        public ResultKind Result
        {
            protected set;
            get;
        }

        /// <summary>
        /// If the trees are unequal, the first node in preorder traversal where the tree differ.
        /// </summary>
        public int DiffersAt
        {
            protected set;
            get;
        }

        /// <summary>
        /// Compares trees using default comparison: node1.Equals(node2).
        /// </summary>
        public bool Compare(TreeT1 tree1, NodeT1 root1, TreeT2 tree2, NodeT2 root2)
        {
            return Compare(tree1, root1, tree2, root2, (t1, n1, t2, n2) => n1.Equals(n2));
        }

        /// <summary>
        /// Compares trees.
        /// </summary>
        public bool Compare(TreeT1 tree1, NodeT1 root1, TreeT2 tree2, NodeT2 root2, CompareDelegate compare)
        {
            if (GetChild1 == null)
            {
                GetChild1 = TreeDefs<TreeT1, NodeT1, IteratorT1>.FindTreeGetChildMethod();
            }
            if (GetChild2 == null)
            {
                GetChild2 = TreeDefs<TreeT2, NodeT2, IteratorT2>.FindTreeGetChildMethod();
            }

            DiffersAt = 0;
            Result = ResultKind.Equal;

            List<StackEntry> stack = new List<StackEntry>(100);
            stack.Add(new StackEntry { Node1 = root1, Node2 = root2 });

            int depth = 0;
            StackEntry context = stack[0];
            for (; ; )
            {
                // Compare the nodes.
                if(!compare(tree1, context.Node1, tree2, context.Node2))
                {
                    Result = ResultKind.ValueDiffers;
                    break;
                }
                DiffersAt++;

                nextChild:

                NodeT1 child1;
                NodeT2 child2;
                bool child1Exists = GetChild1(tree1, context.Node1, ref context.ChildrenIt1, out child1);
                bool child2Exists = GetChild2(tree2, context.Node2, ref context.ChildrenIt2, out child2);
                if(child1Exists != child2Exists)
                {
                    Result = ResultKind.StructureDiffers;
                    break;
                }
                if(child1Exists)
                {
                    depth++;
                    if (stack.Count == depth)
                    {
                        stack.Add(new StackEntry());
                    }
                    context = stack[depth];
                    context.Node1 = child1;
                    context.Node2 = child2;
                    context.ChildrenIt1 = default(IteratorT1);
                    context.ChildrenIt2 = default(IteratorT2);
                }
                else
                {
                    // No more children.

                    // Release references
                    context.Node1 = default(NodeT1);
                    context.Node2 = default(NodeT2);

                    if (--depth < 0)
                        break;
                    context = stack[depth];
                    goto nextChild;
                }
            }

            return Result == ResultKind.Equal;
        }
        #endregion

        #region Implementation

        class StackEntry
        {
            public NodeT1 Node1;
            internal IteratorT1 ChildrenIt1;
            public NodeT2 Node2;
            internal IteratorT2 ChildrenIt2;
        }

        #endregion
    }

    /// <summary>
    /// Helper static methods for CompareTrees&lt;&gt;. Only iterator types are generic parameters of the class,
    /// the other generic parameters can be deduced by the compiler.
    /// </summary> 
    public class CompareTrees<IteratorT1, IteratorT2>: ICompareTreesDefs
    {
        public static bool Compare<TreeT1, NodeT1, TreeT2, NodeT2>(TreeT1 tree1, NodeT1 root1, TreeT2 tree2, NodeT2 root2)
        {
            CompareTrees<TreeT1, NodeT1, IteratorT1, TreeT2, NodeT2, IteratorT2> comparer =
                new CompareTrees<TreeT1, NodeT1, IteratorT1, TreeT2, NodeT2, IteratorT2>();
            return comparer.Compare(tree1, root1, tree2, root2);
        }

        public static bool Compare<TreeT1, NodeT1, TreeT2, NodeT2>(TreeT1 tree1, NodeT1 root1, TreeT2 tree2, NodeT2 root2,
            CompareTrees<TreeT1, NodeT1, IteratorT1, TreeT2, NodeT2, IteratorT2>.CompareDelegate compare)
        {
            CompareTrees<TreeT1, NodeT1, IteratorT1, TreeT2, NodeT2, IteratorT2> comparer =
                new CompareTrees<TreeT1, NodeT1, IteratorT1, TreeT2, NodeT2, IteratorT2>();
            return comparer.Compare(tree1, root1, tree2, root2, compare);
        }

    }
}
