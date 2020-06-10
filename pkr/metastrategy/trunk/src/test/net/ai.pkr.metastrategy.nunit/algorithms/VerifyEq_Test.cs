/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.lib.algorithms;
using System.IO;
using ai.pkr.metastrategy.algorithms;
using System.Reflection;


namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for VerifyEq. 
    /// </summary>
    [TestFixture]
    public unsafe class VerifyEq_Test
    {
        #region Tests

        [Test]
        public void Test_Verify()
        {
            string [] strFiles = new string[] { "eq-KunhPoker-0-s.xml", "eq-KunhPoker-1-s.xml" };

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                    Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            StrategyTree[] st = new StrategyTree[gd.MinPlayers];
            for (int i = 0; i < gd.MinPlayers; ++i)
            {
                string strPath = Path.Combine(_testResDir, strFiles[i]);
                st[i] = XmlToStrategyTree.Convert(strPath, gd.DeckDescr);
            }
            ActionTree at = CreateActionTreeByGameDef.Create(gd);
            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            string error;
            Assert.IsTrue(VerifyEq.Verify(at, ct, st, 0.0000001, out error), error);
            // Now modify the strategy (with a J: not always fold to a r)
            st[1].Nodes[10].Probab = 0.5;
            st[1].Nodes[11].Probab = 0.5;
            Assert.IsFalse(VerifyEq.Verify(at, ct, st, 0.0000001, out error));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());

        #endregion
    }
}
