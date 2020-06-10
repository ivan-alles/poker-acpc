using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using ai.pkr.metagame;
using System.IO;
using ai.lib.algorithms;

namespace equilibrium_lp
{
    /// <summary>
    /// Builds files with illustration to the lp solver.
    /// </summary>
    [TestFixture]
    public class ObviousMove
    {
        #region Tests

        [Test]
        public void Test_BuildFiles()
        {

            string gdFile = Props.Global.Expand("${bds.DataDir}\\ai.pkr.metastrategy.kuhn.gamedef.1.xml");
            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(gdFile);

            string workingDir = Directory.GetCurrentDirectory() + @"..\..\..\..\";

            for (int heroPos = 0; heroPos < 2; heroPos++)
            {
                Console.Write("Game:{0} pos:{1}", gd.Name, heroPos);
                ObviousMoveSolver solver = new ObviousMoveSolver();

                solver.HeroPosition = heroPos;
                solver.GameDef = gd;
                solver.Calculate();

                string fileName = String.Format("hero-tree-{0}.gv", heroPos);
                using (TextWriter tw = new StreamWriter(Path.Combine(workingDir, fileName)))
                {
                    HeroTreeVis vis = new HeroTreeVis {Output = tw, Solver = solver};
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
                    OppTreeVis vis = new OppTreeVis {Output = tw, Solver = solver};
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


        #endregion
    }
}
