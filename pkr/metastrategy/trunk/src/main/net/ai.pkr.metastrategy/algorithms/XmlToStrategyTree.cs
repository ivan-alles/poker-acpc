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
    /// Converts XML tree to PkrDataTree. XML must look like this:
    /// <para>&lt;b&gt;</para>
    /// <para> &lt;d p="0" c="J"&gt;</para>
    /// <para>  &lt;c p="0"&gt;</para>
    /// <para>   &lt;c p="1"/&gt;</para>
    /// <para>  &lt;r p="1" a="1"&gt;</para>
    /// <para>   &lt;f p="0" /&gt;</para>
    /// <para>   &lt;c p="0" /&gt;</para>
    /// <para>   ........</para>
    /// <para>&lt;/b&gt;</para>
    /// <para>An XML element must have the same name as the action character, and p (position), c (cards), a (amount) and probab attributes
    /// must be set (if action requires it).</para>
    /// <para>The algorithm traverses the XML tree and finds the corresponding node in the StrategyTree. The conversion
    /// takes place in OnNodeBeginFunc(). Override it to customize the conversion.</para>
    /// <para>Usage: either create instance and call Walk or call a static method Convert().</para>
    /// </summary>
    public unsafe class XmlToStrategyTree : WalkTreePP<XmlDocument, XmlElement, int, WalkTreePPContext<XmlElement, int>> 
    {

        public XmlToStrategyTree()
        {
            GetChild = XmlTree.GetElementChildren;
        }

        /// <summary>
        /// If set, the card names will be converted from string to indexes.
        /// Otherwise the card names are interpeted as indexes.
        /// </summary>
        public DeckDescriptor DeckDescr
        {
            set;
            get;
        }

        public StrategyTree StrategyTree
        {
            protected set;
            get;
        }


        public static StrategyTree Convert(XmlDocument xml, DeckDescriptor deckDescr)
        {
            XmlToStrategyTree conv = new XmlToStrategyTree { DeckDescr = deckDescr };
            conv.Walk(xml, xml.DocumentElement);
            return conv.StrategyTree;
        }


        public static StrategyTree Convert(XmlDocument xml)
        {
            return Convert(xml, null);
        }

        public static StrategyTree Convert(string xmlFile)
        {
            return Convert(xmlFile, null);
        }

        public static StrategyTree Convert(string xmlFile, DeckDescriptor deckDescr)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlFile);
            return Convert(xml, deckDescr);
        }



        protected override void OnTreeBeginFunc(XmlDocument tree, XmlElement root)
        {
            int nodesCount = tree.SelectNodes("//*").Count;
            StrategyTree = new StrategyTree(nodesCount);
            _nodeId = 0;
        }

        protected override bool OnNodeBeginFunc(XmlDocument tree, XmlElement node, List<WalkTreePPContext<XmlElement, int>> stack, int depth)
        {
            StrategyTree.SetDepth(_nodeId, (byte)depth);

            string actionKindText = node.Name;
            if (actionKindText == "d")
            {
                StrategyTree.Nodes[_nodeId].IsDealerAction = true;
                if (node.HasAttribute("c"))
                {
                    string card = node.GetAttribute("c");
                    if (DeckDescr == null)
                    {
                        StrategyTree.Nodes[_nodeId].Card = int.Parse(card);
                    }
                    else
                    {
                        StrategyTree.Nodes[_nodeId].Card = DeckDescr.GetIndex(card);
                    }
                }
            }
            else if (actionKindText == "p")
            {
                StrategyTree.Nodes[_nodeId].IsDealerAction = false;
                if (node.HasAttribute("a"))
                {
                    StrategyTree.Nodes[_nodeId].Amount = double.Parse(node.GetAttribute("a"), CultureInfo.InvariantCulture);
                }
                if (node.HasAttribute("probab"))
                {
                    StrategyTree.Nodes[_nodeId].Probab = double.Parse(node.GetAttribute("probab"), CultureInfo.InvariantCulture);
                }
            }
            else
            {
                throw new ApplicationException(String.Format("Unknown action: {0}", actionKindText));
            }
            if (node.HasAttribute("p"))
            {
                StrategyTree.Nodes[_nodeId].Position = int.Parse(node.GetAttribute("p"));
            }
            _nodeId++;
            return true;
        }

        protected int _nodeId;
    }
}