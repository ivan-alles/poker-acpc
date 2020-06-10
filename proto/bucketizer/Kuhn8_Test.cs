using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metagame;
using ai.pkr.metastrategy.vis;

namespace bucketizer_proto
{
    /// <summary>
    /// Unit tests for Kuhn8. 
    /// </summary>
    [TestFixture]
    public unsafe class Kuhn8_Test
    {
        #region Tests

        [Test]
        public void Test_Eq()
        {
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>("kuhn8.gamedef.xml");
            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            VisChanceTree.Show(ct, "kuhn8-ct.gv");
            ActionTree at = CreateActionTreeByGameDef.Create(gd);
            VisActionTree.Show(at, "kuhn8-at.gv");



            double[] values;
            StrategyTree [] strategies = EqLp.Solve(at, ct, out values);
            Console.WriteLine("Kuhn8 eq values: {0}, {1}", values[0], values[1]);
            VisStrategyTree.Show(strategies[0], "kuhn8-eq-0.gv");
            VisStrategyTree.Show(strategies[1], "kuhn8-eq-1.gv");

            // Make strategy for T same as for Q 
            //strategies[0].Nodes[strategies[0].FindNode("0d0 0p0", null)].Probab = 0.5;
            //strategies[0].Nodes[strategies[0].FindNode("0d0 0p0 1p1 0p1", null)].Probab = 0.5;
            //strategies[0].Nodes[strategies[0].FindNode("0d0 0p1", null)].Probab = 0.5;


            // Make strategy for Q same as for T 
            strategies[0].Nodes[strategies[0].FindNode("0d2 0p0", null)].Probab = 0;
            strategies[0].Nodes[strategies[0].FindNode("0d2 0p0 1p1 0p1", null)].Probab = 0;
            strategies[0].Nodes[strategies[0].FindNode("0d2 0p1", null)].Probab = 1;

            VisStrategyTree.Show(strategies[0], "kuhn8-eq-0-adj.gv");


            Br br = new Br{ActionTree = at, ChanceTree = ct, HeroPosition = 1};
            br.Strategies = new StrategyTree[] { strategies[0], null };
            br.Solve();
            StrategyTree br0 = br.Strategies[1];
            Console.WriteLine("Br against pos 0: {0}", br.Value);
            VisStrategyTree.Show(strategies[1], "kuhn8-eq-br-0.gv");

        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
