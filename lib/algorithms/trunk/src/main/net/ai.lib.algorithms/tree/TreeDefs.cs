/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Reflection;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Common definitions for universal tree algorithms.
    /// </summary>
    /// <typeparam name="NodeT">Tree node type.</typeparam>
    /// <typeparam name="TreeT">The tree, a place for data and methods shared by all nodes.</typeparam>
    /// <typeparam name="IteratorT">Iterator to access children.  Must allow to sequentially iterate through all 
    /// children in one direction. default(IteratorT) means the first child.</typeparam>
    /// Developer notes:
    /// We could have used an interface to access children, something like ITree, but this will not allow to 
    /// to run this algorithm and derivations with existing tree structures (e.g. XML) or special tree structures, like
    /// tree stored in an array.
    /// TreeT is introduced because it is often neccessary to have common data and methods for all tree nodes, 
    /// this type reperesents this place. This approach often allows to simplify the nodes, because it is not necessary
    /// to keep shared items or references to them in each node.
    /// This class could be an interface, this would allow to derive tree classes from it and so give them types declared here.
    /// But there are classes that use 2 different tree types, and this cannot be solved by such ihnertance, therefore
    /// this is just a class not intended to be inherited from.
    public sealed class TreeDefs<TreeT, NodeT, IteratorT>
    {
        /// <summary>
        /// A delegate returning a child of a node.
        /// </summary>
        public delegate bool GetChildDelegate(TreeT tree, NodeT node, ref IteratorT i, out NodeT child);

        public static GetChildDelegate FindTreeGetChildMethod()
        {
            GetChildDelegate result = null;
            MethodInfo[] methodInfos = typeof(TreeT).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methodInfos.Length; ++i)
            {
                if (methodInfos[i].Name == "TreeGetChild")
                {
                    result = (GetChildDelegate)Delegate.CreateDelegate(typeof(GetChildDelegate), methodInfos[i]);
                    break;
                }
            }
            return result;
        }
    }
}