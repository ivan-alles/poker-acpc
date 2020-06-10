/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using ai.pkr.metagame;
using ai.pkr.metastrategy.nunit;
using System.IO;
using System.Reflection;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for DumpStrategyTree. 
    /// </summary>
    [TestFixture]
    public unsafe class DumpStrategyTree_Test
    {
        #region Tests

        [Test]
        public void Test_Convert()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            string testDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
            string xmlFile = Path.Combine(testDir, "eq-KunhPoker-0-s.xml");

            StrategyTree st1 = XmlToStrategyTree.Convert(xmlFile, gd.DeckDescr);
            //StrategyTreeDump.ToTxt(st1, Console.Out);

            MemoryStream ms = new MemoryStream();
            using (TextWriter tw = new StreamWriter(ms))
            {
                DumpStrategyTree.ToTxt(st1, tw);
            }
            byte[] buf = ms.ToArray();
            ms = new MemoryStream(buf);
            StrategyTree st2;
            using (TextReader tr = new StreamReader(ms))
            {
                st2 = DumpStrategyTree.FromTxt(tr);
            }
            Assert.AreEqual(st1.Version, st2.Version);
            Assert.AreEqual(st1.NodesCount, st2.NodesCount);
            for (Int64 n = 0; n < st2.NodesCount; ++n)
            {
                Assert.AreEqual(st1.GetDepth(n), st2.GetDepth(n));
                Assert.AreEqual(st1.Nodes[n].Position, st2.Nodes[n].Position);
                Assert.AreEqual(st1.Nodes[n].IsDealerAction, st2.Nodes[n].IsDealerAction);
                if (st1.Nodes[n].IsDealerAction)
                {
                    Assert.AreEqual(st1.Nodes[n].Card, st2.Nodes[n].Card);
                }
                else
                {
                    Assert.AreEqual(st1.Nodes[n].Amount, st2.Nodes[n].Amount);
                    Assert.AreEqual(st1.Nodes[n].Probab, st2.Nodes[n].Probab);
                }

            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
