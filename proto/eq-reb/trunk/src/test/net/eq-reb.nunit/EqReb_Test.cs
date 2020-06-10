using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.vis;
using System.IO;
using System.Reflection;

namespace eq_reb.nunit
{
    /// <summary>
    /// Unit tests for EqReb. 
    /// </summary>
    [TestFixture]
    public class EqReb_Test
    {
        #region Tests

        [Test]
        public void Test_Kuhn()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/kuhn.gamedef.xml"));

            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            ActionTree at = CreateActionTreeByGameDef.Create(gd);
            StrategyTree [] playerTrees = new StrategyTree[2];
            for (int p = 0; p < 2; ++p)
            {
                ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ct, p);
                StrategyTree st = CreateStrategyTreeByChanceAndActionTrees.CreateS(pct, at);
                playerTrees[p] = st;

                VisStrategyTree.Show(st, Path.Combine(_outDir, string.Format("pt-{0}.gv", p)));
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "EqReb_Test");


        #endregion
    }
}
