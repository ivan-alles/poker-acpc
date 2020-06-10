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
    /// Unit tests for DumpChanceTree. 
    /// </summary>
    [TestFixture]
    public unsafe class DumpChanceTree_Test
    {
        #region Tests

        [Test]
        public void Test_Convert()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/leduc-he.gamedef.xml"));

            string testDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());

            ChanceTree ct1 = CreateChanceTreeByGameDef.Create(gd);
            // DumpChanceTree.ToTxt(ct1, Console.Out);

            MemoryStream ms = new MemoryStream();
            using (TextWriter tw = new StreamWriter(ms))
            {
                DumpChanceTree.ToTxt(ct1, tw);
            }
            byte[] buf = ms.ToArray();
            ms = new MemoryStream(buf);
            ChanceTree ct2;
            using (TextReader tr = new StreamReader(ms))
            {
                ct2 = DumpChanceTree.FromTxt(tr);
            }
            Assert.AreEqual(ct1.Version, ct2.Version);
            Assert.AreEqual(ct1.NodesCount, ct2.NodesCount);
            int roundsCount = ct1.CalculateRoundsCount();
            double[] potShare1 = new double[ct1.PlayersCount];
            double[] potShare2 = new double[ct1.PlayersCount];
            for (Int64 n = 0; n < ct2.NodesCount; ++n)
            {
                Assert.AreEqual(ct1.GetDepth(n), ct2.GetDepth(n));
                Assert.AreEqual(ct1.Nodes[n].Position, ct2.Nodes[n].Position);
                Assert.AreEqual(ct1.Nodes[n].Card, ct2.Nodes[n].Card);
                Assert.AreEqual(ct1.Nodes[n].Probab, ct2.Nodes[n].Probab);
                if (ct1.GetDepth(n) == ct1.PlayersCount * roundsCount)
                {
                    UInt16 [] activePlayerMasks = ActivePlayers.Get(ct1.PlayersCount, 2, ct1.PlayersCount);
                    foreach (UInt16 ap in activePlayerMasks)
                    {
                        ct1.Nodes[n].GetPotShare(ap, potShare1);
                        ct2.Nodes[n].GetPotShare(ap, potShare2);
                        Assert.AreEqual(potShare1, potShare2);
                    }
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
