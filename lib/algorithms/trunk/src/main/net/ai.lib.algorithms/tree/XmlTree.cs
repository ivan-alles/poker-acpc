/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Helper functions to bind .Net XML to tree algorithms.
    /// </summary>
    public static class XmlTree
    {
        /// <summary>
        /// Gets only children of type XmlElement.
        /// </summary>
        public static bool GetElementChildren(XmlDocument tree, XmlElement n, ref int i, out XmlElement child)
        {
            child = null;
            for (; i < n.ChildNodes.Count; )
            {
                child = n.ChildNodes[i] as XmlElement;
                ++i;
                if (child != null)
                {
                    break;
                }
            }
            return child != null;
        }
    }
}
