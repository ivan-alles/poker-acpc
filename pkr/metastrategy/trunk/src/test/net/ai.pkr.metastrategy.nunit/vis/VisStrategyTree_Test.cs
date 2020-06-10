/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.algorithms.tree;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy;
using System.IO;
using ai.pkr.metastrategy.algorithms;
using System.Reflection;
using ai.pkr.metastrategy.nunit;

namespace ai.pkr.metastrategy.vis.nunit
{
    /// <summary>
    /// Unit tests for VisStrategyTree. 
    /// As it is difficult to verify by coding, just let it run and watch.
    /// </summary>
    [TestFixture]
    public class VisStrategyTree_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            ShowTree(gd);
        }

        [Test]
        public void Test_LeducHe()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));
            ShowTree(gd);
        }

        #endregion

        #region Implementation

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "vis/VisStrategyTree_Test");

        private void ShowTree(GameDefinition gd)
        {
            for (int pos = 0; pos < gd.MinPlayers; ++pos)
            {
                StrategyTree st = TreeHelper.CreateStrategyTree(gd, pos);

                string fileName = string.Format("{0}-{1}.gv", gd.Name, pos);
                using (TextWriter w = new StreamWriter(File.Open(Path.Combine(_outDir, fileName), FileMode.Create)))
                {
                    VisStrategyTree vis = new VisStrategyTree { Output = w, CardNames = gd.DeckDescr.CardNames };
                    vis.Show(st, 3);
                }
            }
        }

        #endregion
    }
}
