/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms.tree;
using ai.lib.utils;
using System.Reflection;
using ai.lib.algorithms;
using System.IO;
using System.Xml;
using ai.pkr.metagame;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Writes a poker tree to an XML file. By default, prints action kind, position, amount, cards and probability.
    /// (only valid values), for example &lt;d p="0" c="J"&gt;.
    /// Use ShowExpr to customize the XML content.
    /// </summary>
    public unsafe class StrategyTreeToXml : XmlizeTree<UFToUniAdapter, int, int, XmlizeTreeContext<int, int>>
    {
        public StrategyTreeToXml()
        {
            // Add this assembly
            CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));

            ShowExpr.Add(new ExprFormatter("((ai.pkr.metastrategy.algorithms.StrategyTreeToXml)c).GetNodeName(t, s, d)", "{1}"));
            ShowExpr.Add(new ExprFormatter("((ai.pkr.metastrategy.algorithms.StrategyTreeToXml)c).GetPositionAttribute(t, s, d)", "a;p;{1}"));
            ShowExpr.Add(new ExprFormatter("((ai.pkr.metastrategy.algorithms.StrategyTreeToXml)c).GetCardAttribute(t, s, d)", "a;c;{1}"));
            ShowExpr.Add(new ExprFormatter("((ai.pkr.metastrategy.algorithms.StrategyTreeToXml)c).GetAmountAttribute(t, s, d)", "a;a;{1}"));
            ShowExpr.Add(new ExprFormatter("((ai.pkr.metastrategy.algorithms.StrategyTreeToXml)c).GetProbabAttribute(t, s, d)", "a;probab;{1}"));
            
            SkipEmpty = true;
        }


        /// <summary>
        /// If set, the card names will be converted from indexes to strings.
        /// Otherwise the card names will be written as indexes.
        /// </summary>
        public DeckDescriptor DeckDescr
        {
            set;
            get;
        }
        
        /// <summary>
        /// Converts a given tree. The convertion is controlled by the properties.
        /// </summary>
        /// <param name="t"></param>
        public void Convert(StrategyTree t)
        {
            UFToUniAdapter adapter = new UFToUniAdapter(t);
            Walk(adapter, 0);
        }

        /// <summary>
        /// A shortcut to convert to an XML file.
        /// </summary>
        public static void Convert(StrategyTree t, string fileName, DeckDescriptor deckDescr)
        {
            using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create)))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.ASCII;
                settings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(w, settings))
                {
                    StrategyTreeToXml xmlizer = new StrategyTreeToXml { Output = xmlWriter, DeckDescr = deckDescr };
                    xmlizer.Convert(t);
                }
            }
        }


        public virtual string GetNodeName(UFToUniAdapter t, List<XmlizeTreeContext<int, int>> s, int d)
        {
            return ((StrategyTree)t.UfTree).Nodes[s[d].Node].IsDealerAction ? "d" : "p";
        }

        public virtual string GetPositionAttribute(UFToUniAdapter t, List<XmlizeTreeContext<int, int>> s, int d)
        {
            return ((StrategyTree)t.UfTree).Nodes[s[d].Node].Position.ToString();
        }

        public virtual string GetAmountAttribute(UFToUniAdapter t, List<XmlizeTreeContext<int, int>> s, int d)
        {
            return ((StrategyTree)t.UfTree).Nodes[s[d].Node].IsDealerAction ? "" : ((StrategyTree)t.UfTree).Nodes[s[d].Node].Amount.ToString();
        }

        public virtual string GetProbabAttribute(UFToUniAdapter t, List<XmlizeTreeContext<int, int>> s, int d)
        {
            return ((StrategyTree)t.UfTree).Nodes[s[d].Node].IsDealerAction ? "" : ((StrategyTree)t.UfTree).Nodes[s[d].Node].Probab.ToString();
        }

        public virtual string GetCardAttribute(UFToUniAdapter t, List<XmlizeTreeContext<int, int>> s, int d)
        {
            StrategyTree st = (StrategyTree)t.UfTree;
            if (!st.Nodes[s[d].Node].IsDealerAction)
            {
                return "";
            }
            int card = st.Nodes[s[d].Node].Card;
            string cardText = DeckDescr != null ? DeckDescr.CardNames[card] : card.ToString();
            return cardText;
        }
    }
}
