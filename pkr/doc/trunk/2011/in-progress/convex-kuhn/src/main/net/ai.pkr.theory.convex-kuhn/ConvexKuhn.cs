using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using ai.pkr.metagame;
using System.IO;
using ai.lib.algorithms;
using System.Xml;
using System.Globalization;

namespace convex_kuhn
{
    /// <summary>
    /// Builds files with illustration to the lp solver.
    /// </summary>
    [TestFixture]
    public class ConvexKuhn
    {
        #region Tests

        [Test]
        public void Test_BuildFiles()
        {

            string gdFile = Props.Global.Expand("${bds.DataDir}\\ai.pkr.metastrategy.kuhn.gamedef.1.xml");
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(gdFile);

            string workingDir = Directory.GetCurrentDirectory() + @"..\..\..\..\";

            string strFile0 = Path.Combine("data", "kuhn-s-0.xml");
            string strFile1 = Path.Combine("data", "kuhn-s-1.xml");

            for (int heroPos = 1; heroPos < 2; heroPos++)
            {
                Console.Write("Game:{0} pos:{1}", gd.Name, heroPos);

                XmlStrategyHelper strHelper = new XmlStrategyHelper();

                strHelper.GameDef = gd;
                strHelper.HeroPosition = heroPos;

                strHelper.StrategyFiles = new string[] { strFile0, strFile1 };
                strHelper.LoadStrategies();

                Calculator solver = new Calculator();

                solver.HeroPosition = heroPos;
                solver.GameDef = gd;
                solver.Calculate();

                string fileName = String.Format("hero-tree-{0}.gv", heroPos);
                using (TextWriter tw = new StreamWriter(Path.Combine(workingDir, fileName)))
                {
                    HeroTreeVis vis = new HeroTreeVis {Output = tw, Solver = solver, FlatStrategy = strHelper.FlatStrategy};
                    vis.MergePrivateDeals = true;
                    vis.GraphAttributes.Map["fontname"] = "arial";

                    vis.GraphAttributes.fontsize = 10;
                    vis.EdgeAttributes.fontsize = 10;
                    vis.NodeAttributes.fontsize = 10;

                    vis.Walk(solver.PlayerTrees[heroPos]);
                }


                fileName = String.Format("opp-tree-{0}.gv", heroPos); 
                using (TextWriter tw = new StreamWriter(Path.Combine(workingDir, fileName)))
                {
                    OppTreeVis vis = new OppTreeVis { Output = tw, Solver = solver, FlatStrategy = strHelper.FlatStrategy };
                    vis.MergePrivateDeals = true;
                    vis.GraphAttributes.Map["fontname"] = "arial";

                    vis.GraphAttributes.fontsize = 10;
                    vis.EdgeAttributes.fontsize = 10;
                    vis.NodeAttributes.fontsize = 10;

                    vis.Walk(solver.PlayerTrees[1 - heroPos]);
                }
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        public class XmlStrategyHelper
        {

            public int HeroPosition;

            public string[] StrategyFiles;


            public GameDefinition GameDef;

            protected void AddStrategicProbability(int id, string probability)
            {
                while (FlatStrategy.Count <= id)
                {
                    FlatStrategy.Add("");
                }
                FlatStrategy[id] = probability;
            }

            // Flattened strategy
            public List<string> FlatStrategy = new List<string>();

            XmlDocument[] Strategies
            {
                set;
                get;
            }

            public void LoadStrategies()
            {
                Strategies = new XmlDocument[] { new XmlDocument(), new XmlDocument() };
                Strategies[0].Load(StrategyFiles[0]);
                Strategies[1].Load(StrategyFiles[1]);
                FlatStrategy.Clear();
                SetProbability(Strategies[HeroPosition].DocumentElement);
            }

            private void SetProbability(XmlElement strategyNode)
            {
                int pos = HeroPosition;

                string probab = "";
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
                            string sumSiblings = "";
                            foreach (XmlNode child in parent.ChildNodes)
                            {
                                // Skip this node
                                if (object.ReferenceEquals(child, strategyNode))
                                    continue;
                                // Id is useful for debugging.
                                string id = ((XmlElement)child).GetAttribute("id");
                                string probabText = ((XmlElement)child).GetAttribute("probab");
                                sumSiblings += probabText;
                            }
                            if (sumSiblings == "1")
                            {
                                probab = "0";
                            }
                            else
                            {
                                probab = "1 - " + sumSiblings;
                            }

                        }
                        else
                        {
                            probab = strategyNode.GetAttribute("probab");
                        }
                        AddStrategicProbability(xmlId, probab);
                    }
                }


                foreach (XmlNode strategyChild in strategyNode.ChildNodes)
                {
                    if (!(strategyChild is XmlElement))
                        continue;
                    SetProbability((XmlElement)strategyChild);
                }
            }
        }


        #endregion
    }
}
