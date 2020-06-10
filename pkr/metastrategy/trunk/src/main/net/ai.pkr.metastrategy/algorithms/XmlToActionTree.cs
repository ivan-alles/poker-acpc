/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using System.Xml;
using ai.pkr.metagame;
using System.Globalization;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Converts XML tree to action tree.
    /// <para>The algorithm traverses the XML tree and finds the corresponding node in the ActionTree. The conversion
    /// takes place in OnNodeBeginFunc(). Override it to customize the conversion.</para>
    /// <para>Usage: either create instance and call Walk or call a static method Convert().</para>
    /// </summary>
    public unsafe class XmlToActionTree : WalkTreePP<XmlDocument, XmlElement, int, WalkTreePPContext<XmlElement, int>> 
    {

        public XmlToActionTree()
        {
            GetChild = XmlTree.GetElementChildren;
        }

        public ActionTree ActionTree
        {
            protected set;
            get;
        }


        public static ActionTree Convert(XmlDocument xml)
        {
            XmlToActionTree conv = new XmlToActionTree();
            conv.Walk(xml, xml.DocumentElement);
            return conv.ActionTree;
        }


        public static ActionTree Convert(string xmlFile)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlFile);
            return Convert(xml);
        }

        protected override void OnTreeBeginFunc(XmlDocument tree, XmlElement root)
        {
            int nodesCount = tree.SelectNodes("//*").Count;
            ActionTree = new ActionTree(nodesCount);
            _nodeId = 0;
        }

        protected override bool OnNodeBeginFunc(XmlDocument tree, XmlElement node, List<WalkTreePPContext<XmlElement, int>> stack, int depth)
        {
            ActionTree.SetDepth(_nodeId, (byte)depth);

            string actionKindText = node.Name;
            if (actionKindText == "p")
            {
                if (node.HasAttribute("a"))
                {
                    ActionTree.Nodes[_nodeId].Amount = double.Parse(node.GetAttribute("a"), CultureInfo.InvariantCulture);
                }
            }
            else
            {
                throw new ApplicationException(String.Format("Unknown action: {0}", actionKindText));
            }
            if (node.HasAttribute("p"))
            {
                ActionTree.Nodes[_nodeId].Position = int.Parse(node.GetAttribute("p"));
            }
            if (node.HasAttribute("r"))
            {
                ActionTree.Nodes[_nodeId].Round = int.Parse(node.GetAttribute("r"));
            }
            if (node.HasAttribute("ap"))
            {
                ActionTree.Nodes[_nodeId].ActivePlayers = ushort.Parse(node.GetAttribute("ap"));
            }

            _nodeId++;
            return true;
        }

        protected int _nodeId;
    }
}