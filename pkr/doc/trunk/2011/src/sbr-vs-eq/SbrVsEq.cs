/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using System.IO;
using System.Reflection;
using System.Xml;
using ai.lib.utils;
using System.Globalization;
using System.Text.RegularExpressions;
using ai.lib.algorithms;
using ai.pkr.metastrategy;


namespace ai.pkr.theory.sbr_vs_eq
{
    /// <summary>
    /// Prepares artifacts showing why it is possible to have a choice of a best move
    /// in SBR against an equilibrium strategy.
    /// </summary>
    [TestFixture]
    public class SbrVsEq
    {
        #region Tests

        [Test]
        public void Test_GenerateFiles()
        {
            XmlStrategyHelper strHelper = new XmlStrategyHelper();

            string dataDir = Props.Global.Get("bds.DataDir");
            string workingDir = Directory.GetCurrentDirectory() + @"..\..\..\..\";

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Path.Combine(dataDir, "ai.pkr.metastrategy.kuhn.gamedef.1.xml"));

            bool[] normSuits = new bool[] {false, true};

            int pos = 0;
            strHelper.HeroPosition = pos;

            strHelper.StrategyFiles = new string[] { null, Path.Combine(workingDir, "kuhn-s-1-eq.xml") };
            strHelper.LoadStrategies();
            Sbr brEq = FindBestResponse(strHelper, pos, gd, null);

            Assert.AreEqual(-1.0 / 18, brEq.PlayerTrees[pos].Value, 0.00000001);
            Assert.AreEqual(1.0, brEq.PlayerTrees[pos].Probab, 0.00000001);

            // Nodes 3 and 8 have equal values, but we would prefer to chose node 3 for this
            // demonstration.
            brEq.PlayerTrees[pos].Children[0].Children[0].BestActionIndex = 0;

            using (TextWriter tw = new StreamWriter(Path.Combine(workingDir, "br-eq-p0.gv")))
            {
                VisBr visBr = new VisBr { Output = tw };
                SetVisAttributes(visBr);
                //visBr.NodeAttributes.margin = "0.001,0.005";
                visBr.ShowExpr.Add(new ExprFormatter("s[d].Node.Id", "id:{1}"));
                visBr.ShowExpr.Add(new ExprFormatter("-s[d].Node.Value",  "\\nv:{1:0.000}"));
                visBr.Walk(brEq.PlayerTrees[pos]);
            }

        }

        private void SetVisAttributes(Sbr.Visualizer vis)
        {
            vis.GraphAttributes.Map["fontname"] = "arial";
            vis.GraphAttributes.fontsize = 10;
            vis.NodeAttributes.fontsize = 9;
            vis.EdgeAttributes.fontsize = 10;
        }

        #endregion


        #region Implementation

        class VisBr : Sbr.Visualizer
        {
            public VisBr()
            {
                MergePrivateDeals = true;
            }

            protected override bool OnNodeBeginFunc(Sbr.TreeNode tree, Sbr.TreeNode node, List<Context> stack, int depth)
            {
                bool result = base.OnNodeBeginFunc(tree, node, stack, depth);
                return result;
            }

            protected override void CustomizeNodeAttributes(Sbr.TreeNode tree, Sbr.TreeNode node, List<Context> stack, int depth, ai.lib.algorithms.tree.VisTree<Sbr.TreeNode, Sbr.TreeNode, int, Context>.NodeAttributeMap attr)
            {
                base.CustomizeNodeAttributes(tree, node, stack, depth, attr);
            }

        }

        /// <summary>
        /// This code is copypasted from StaticBestResponse_Test.
        /// </summary>
        public abstract class StrategyHelper
        {
            public int HeroPosition;

            public string[] StrategyFiles;

            public abstract void LoadStrategies();

            public abstract void SetProbabilityOfOppAction();

            public GameDefinition GameDef;

            protected void AddStrategicProbability(int id, double probability)
            {
                while (OppStrategy.Count <= id)
                {
                    OppStrategy.Add(0);
                }
                OppStrategy[id] = probability;
            }

            // Flattened opponent strategy
            public List<double> OppStrategy = new List<double>();

        }

        /// <summary>
        /// This code is copypasted from StaticBestResponse_Test.
        /// </summary>
        public class XmlStrategyHelper : StrategyHelper
        {
            XmlDocument[] Strategies
            {
                set;
                get;
            }

            public override void LoadStrategies()
            {
                Strategies = new XmlDocument[] { new XmlDocument(), new XmlDocument() };
                if (StrategyFiles[0] != null) Strategies[0].Load(StrategyFiles[0]);
                if (StrategyFiles[1] != null) Strategies[1].Load(StrategyFiles[1]);
            }

            public override void SetProbabilityOfOppAction()
            {
                OppStrategy.Clear();
                SetProbability(Strategies[1 - HeroPosition].DocumentElement, 1.0);
            }

            private void SetProbability(XmlElement strategyNode, double absoluteProbability)
            {
                int pos = 1 - HeroPosition;

                double localStrProbab = 1;
                int xmlId = int.Parse(strategyNode.GetAttribute("id"));
                if (strategyNode.HasAttribute("p"))
                {
                    int xmlPos = int.Parse(strategyNode.GetAttribute("p"));
                    if (xmlPos == pos &&
                        (strategyNode.Name == "r" || strategyNode.Name == "c" || strategyNode.Name == "f"))
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
                        AddStrategicProbability(xmlId, localStrProbab * absoluteProbability);
                    }
                }


                foreach (XmlNode strategyChild in strategyNode.ChildNodes)
                {
                    if (!(strategyChild is XmlElement))
                        continue;
                    SetProbability((XmlElement)strategyChild, localStrProbab * absoluteProbability);
                }
            }

            protected virtual double GetLocalStrProbab(string probabText)
            {
                double localStrProbab = double.Parse(probabText, CultureInfo.InvariantCulture);
                return localStrProbab;
            }
        }

        private Sbr FindBestResponse(StrategyHelper strHelper, int pos, GameDefinition gd,
            Sbr.CreateRootGenNodeDelegate createRootGenNodeDelegate)
        {
            Sbr br = new Sbr();

            br.GameDef = gd;
            br.HeroPosition = pos;

            if (createRootGenNodeDelegate != null)
            {
                br.CreateRootGenNode = createRootGenNodeDelegate;
            }

            strHelper.SetProbabilityOfOppAction();
            br.SetOppStrategy(1 - pos, strHelper.OppStrategy.ToArray());
            br.Calculate();

            return br;
        }

        #endregion
    }
}
