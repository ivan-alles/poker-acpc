/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using ai.pkr.metastrategy;
using System.Xml;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.lib.algorithms.tree;
using System.Globalization;

namespace ai.pkr.research.eqbr
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    [Explicit]
    public class EqBr_Test
    {
        #region Tests

        [Test]
        public void Test_EqBr()
        {
            string baseDir = "eqbr";

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Path.Combine(baseDir, "eq-br-gd.xml"));

            string strFile1 = Path.Combine(baseDir, "str-1.xml");

            Strategies = new XmlDocument[] { null, new XmlDocument() }; 
            Strategies[1].Load(strFile1);
            CreateRootGenNode = tree => new SuitlessGenNode(tree, 2);
           
            _oppPos = 0;

            FindBestResponse(baseDir, gd);
        }

        [Test]
        public void Test_Kuhn()
        {
            string baseDir = "eqbr-kuhn";

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                            Path.Combine(baseDir, "..\\..\\data\\ai.pkr.metastrategy.kuhn.gamedef.1.xml"));

            Strategies = new XmlDocument[] { new XmlDocument(), new XmlDocument() };
            Strategies[0].Load(Path.Combine(baseDir, "str-0.xml"));
            Strategies[1].Load(Path.Combine(baseDir, "str-1.xml"));
            CreateRootGenNode = tree => new SuitlessGenNode(tree, 2);
            _oppPos = 0;
            FindBestResponse(baseDir, gd);
            _oppPos = 1;
            FindBestResponse(baseDir, gd);
        }


        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        int _oppPos = 0;

        private BestResponseStatic.CreateRootGenNodeDelegate CreateRootGenNode;

        public XmlDocument[] Strategies
        {
            set;
            get;
        }

        private void FindBestResponse(string baseDir, GameDefinition gd)
        {
            BestResponseStatic bestResp = new BestResponseStatic();
            bestResp.GameDef = gd;
            bestResp.HeroPosition = _oppPos;
            if (CreateRootGenNode != null)
            {
                bestResp.CreateRootGenNode = CreateRootGenNode;
            }

            bestResp.CreateTrees();
            SetProbabilityOfOppAction(bestResp.PlayerTrees[1 - _oppPos], Strategies[1 - _oppPos].DocumentElement);
            bestResp.Calculate();

            string fileName;
            for (int perspective = 0; perspective < 2; ++perspective)
            {
                fileName = Path.Combine(baseDir, string.Format("br-{0}-p{1}.gv", 1-_oppPos, perspective));
                using (TextWriter tw = new StreamWriter(fileName))
                {
                    BestResponseStatic.Visualizer vis = new BestResponseStatic.Visualizer
                                                            {
                                                                Output = tw
                                                            };
                    vis.SetDefaultAttrbutes(bestResp, perspective);
                    vis.Walk(bestResp.PlayerTrees[perspective]);
                }
            }
            fileName = Path.Combine(baseDir, string.Format("br-{0}-g.gv", 1 - _oppPos));
            using (TextWriter tw = new StreamWriter(fileName))
            {
                BestResponseStatic.Visualizer vis = new BestResponseStatic.Visualizer
                                                        {
                                                            Output = tw
                                                        };
                vis.SetDefaultAttrbutes(bestResp, -1);
                vis.Walk(bestResp.GameTree);
            }
        }

        protected virtual double GetLocalStrProbab(string probabText)
        {
            double localStrProbab = double.Parse(probabText, CultureInfo.InvariantCulture);
            return localStrProbab;
        }

        void SetProbabilityOfOppAction(BestResponseStatic.TreeNode node, XmlElement strategyNode)
        {
            int pos = 1 - _oppPos;

            double localStrProbab = 1;
            if (node.Action.Position == pos && node.Action.IsPlayerAction())
            {

                if (!strategyNode.HasAttribute("probab"))
                {
                    // No probability specified - take (1 - sum-other-siblings) 
                    XmlElement parent = (XmlElement)strategyNode.ParentNode;
                    double sumSiblings = 0;
                    foreach (XmlNode child in parent.ChildNodes)
                    {
                        // Skip this node
                        if (object.ReferenceEquals(child, strategyNode))
                            continue;
                        if (!(child is XmlElement))
                            continue;
                        // Id is useful for debugging.
                        string id = ((XmlElement)child).GetAttribute("id");
                        string probabText = ((XmlElement)child).GetAttribute("probab");
                        sumSiblings += GetLocalStrProbab(probabText);
                    }
                    localStrProbab = 1 - sumSiblings;
                }
                else
                {
                    string probabText = strategyNode.GetAttribute("probab");
                    localStrProbab = GetLocalStrProbab(probabText);
                }
            }
            node.StrategicProbab *= localStrProbab;


            for (int c = 0; c < node.Children.Count; ++c)
            {
                BestResponseStatic.TreeNode child = node.Children[c];
                string query = child.Action.Kind.ToString();
                if (child.Action.Kind == Ak.d && child.Action.Position == pos)
                {
                    query += string.Format("[@c='{0}']", child.Action.Cards);
                }
                XmlElement strategyChild = (XmlElement)strategyNode.SelectSingleNode(query);
                child.StrategicProbab = node.StrategicProbab;
                SetProbabilityOfOppAction(child, strategyChild);
            }
        }

        #endregion
    }
}
