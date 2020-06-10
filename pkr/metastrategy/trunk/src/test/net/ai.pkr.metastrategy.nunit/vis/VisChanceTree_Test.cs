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

namespace ai.pkr.metastrategy.vis.nunit
{
    /// <summary>
    /// Unit tests for VisChanceTree. 
    /// As it is difficult to verify by coding, just let it run and watch.
    /// </summary>
    [TestFixture]
    public class VisChanceTree_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            using (TextWriter w = new StreamWriter(File.Open(Path.Combine(_outDir, "kuhn-ct.gv"), FileMode.Create)))
            {
                VisChanceTree vis = new VisChanceTree { Output = w, CardNames = gd.DeckDescr.CardNames };
                vis.Show(ct);
            }
        }

        [Test]
        public void Test_LeducHe()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));

            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);

            using (TextWriter w = new StreamWriter(File.Open(Path.Combine(_outDir, "leduc-ct.gv"), FileMode.Create)))
            {
                VisChanceTree vis = new VisChanceTree { Output = w, CardNames = gd.DeckDescr.CardNames };
                vis.Show(ct);
            }

        }

        #endregion

        #region Implementation

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "vis/VisChanceTree_Test");

        #endregion
    }
}
