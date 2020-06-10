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
    /// Base class for xmlize tree context.
    /// </summary>
    public class XmlizeTreeContext<NodeT, IteratorT> : TextualizeTreeContext<NodeT, IteratorT>
    {
    }

    /// <summary>
    /// Writes a tree to an XML file. 
    /// <para>Control the content of the XML file by using ShowExpr property.</para>
    /// <para>ShowExpr[0] defines the name of the XML element of the current tree node.</para>
    /// <para>The rest ShowExpr[1..N] define the attributes and subelements of the XML element.
    /// The ExprFormatter.Format is the following:</para>
    /// <para>
    /// &lt;type&gt;;&lt;name&gt;;&lt;value&gt; where type is "a" for an attribute, "e" for element.</para>
    /// <para>For example: xt.ShowExpr.Add(new ExprFormatter("s[d].Node.Array[2]", "a;Arr2;{1}"));</para>
    /// </summary>
    public class XmlizeTree<TreeT, NodeT, IteratorT, ContextT> : TextualizeTree<TreeT, NodeT, IteratorT, ContextT> where ContextT : TextualizeTreeContext<NodeT, IteratorT>, new()
    {
        public XmlWriter Output
        {
            set;
            get;
        }

        /// <summary>
        /// Skip empty elements and attributes, default: false.
        /// </summary>
        public bool SkipEmpty
        {
            set;
            get;
        }

        protected override void OnTreeBeginFunc(TreeT tree, NodeT root)
        {
            Output.WriteStartDocument();
        }

        protected override void OnTreeEndFunc(TreeT tree, NodeT root)
        {
            Output.WriteEndDocument();
        }

        protected override bool OnNodeBeginFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            string elementName = "el";
            if(ShowExpr.Count > 0)
            {
                elementName = EvalExpressions(tree, stack, depth, 0, 1);
            }
            // Write the element of the node.
            Output.WriteStartElement(elementName);
            string type, name, value;
            // Write attributes.
            for(int expr = 1; expr < ShowExpr.Count; ++expr)
            {
                string text = EvalExpressions(tree, stack, depth, expr, 1);
                ParseShowExpr(text, out type, out name, out value);
                if (type == "a" && !(SkipEmpty && string.IsNullOrEmpty(value)))
                {
                    Output.WriteAttributeString(name, value);
                }
            }
            // Write values.
            for (int expr = 1; expr < ShowExpr.Count; ++expr)
            {
                string text = EvalExpressions(tree, stack, depth, expr, 1);
                ParseShowExpr(text, out type, out name, out value);
                if (type == "e" && !(SkipEmpty && string.IsNullOrEmpty(value)))
                {
                    Output.WriteStartElement(name);
                    Output.WriteValue(value);
                    Output.WriteEndElement();
                }
            }
            return true;
        }

        /// <summary>
        /// Parses ShowExpr format for attributes and elements.
        /// </summary>
        protected void ParseShowExpr(string text, out string type, out string name, out string value)
        {
            type = text.Substring(0,1);
            int pos = text.IndexOf(';', 2);
            name = text.Substring(2, pos - 2);
            value = text.Substring(pos+1);
        }

        protected override void OnNodeEndFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            Output.WriteEndElement();
        }
    }

}
