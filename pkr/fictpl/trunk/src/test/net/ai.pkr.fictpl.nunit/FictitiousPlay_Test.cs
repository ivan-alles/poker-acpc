/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Runtime.InteropServices;
using ai.pkr.metastrategy;
using ai.pkr.metagame;
using ai.pkr.metastrategy.model_games;
using ai.lib.utils;
using System.Reflection;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metastrategy.vis;
using System.IO;

namespace ai.pkr.fictpl.nunit
{
    /// <summary>
    /// Unit tests for FictitiousPlay. 
    /// </summary>
    [TestFixture]
    public class FictitiousPlay_Test
    {
        public FictitiousPlay_Test()
        {
            //Console.WriteLine("Turn on unmanaged memory diagnostics");
            //UnmanagedMemory.IsDiagOn = true;
        }

        #region Tests for subclasses

        [Test]
        public void Test_Node()
        {
            int size = Marshal.SizeOf(typeof(FictitiousPlay.Node));
            Assert.AreEqual(13, size);
        }

        #endregion

        #region Gamedef tests

        [Test]
        public void Test_Kuhn()
        {
            var testParams = new GameDefParams(this, "kuhn.gamedef.xml", 0.001);
            SolveAndVerifyVerifySolution(testParams, true, false,
                new int[] { 100, -1 },
                s =>
                  {
                      s.IsVerbose = true;
                      s.EpsilonLogThreshold = 0;
                      s.IterationVerbosity = 1000;
                      //s.MaxIterationCount = 2000;
                      s.ThreadsCount = 0; // Test single-threaded variant.
                  });
        }

        [Test]
        public void Test_Kuhn2()
        {
            var testParams = new GameDefParams(this, "kuhn2.gamedef.xml",
                0.001);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[]{100, - 1}, null);
        }

        [Test]
        public void Test_Ocp()
        {
            var testParams = new GameDefParams(this, "ocp.gamedef.xml",
                0.001);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        [Test]
        public void Test_Ocp2()
        {
            var testParams = new GameDefParams(this, "ocp2.gamedef.xml",
                0.001);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }


        [Test]
        public void Test_LeducHe()
        {
            var testParams = new GameDefParams(this, "leduc-he.gamedef.xml",
                _isDebug ? 0.01 : 0.002);
            SolveAndVerifyVerifySolution(testParams, true, false,
                //new int[] { -1 }, 
                new int[] { 5000, -1 }, 
                s =>
                                                      {
                                                          s.IsVerbose = true; // Test verbosity too.
                                                          s.EpsilonLogThreshold = 0;
                                                          s.IterationVerbosity = 10000;
                                                          // s.ThreadsCount = 7;
                                                          //s.MaxIterationCount = 320;
                                                      });

        }

        [Test]
        public void Test_LeducHeRb()
        {
            var testParams = new GameDefParams(this, "leduc-he-rb.gamedef.xml",
                _isDebug ? 0.01 : 0.003);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        [Test]
        public void Test_MicorFl()
        {
            var testParams = new GameDefParams(this, "micro-fl.gamedef.xml",
                _isDebug ? 0.01 : 0.005);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        [Test]
        public void Test_MiniFl()
        {
            var testParams = new GameDefParams(this, "mini-fl.gamedef.xml",
                0.001);
            // Use a big iteration count for the snapshot, it is a good test for
            // initial setting of varless nodes.
            SolveAndVerifyVerifySolution(testParams, true, false, new int[] { 30000, -1 },
                s =>
            {
              s.IsVerbose = true;
              s.EpsilonLogThreshold = 0;
              s.IterationVerbosity = 10000;
              //s.MaxIterationCount = 2;
            });
        }

        #endregion

        #region Abstract tests


        [Test]
        public void Test_Kuhn_CA()
        {
            ChanceAbstractedGameDefParams testParams = new ChanceAbstractedGameDefParams(this, "Kuhn-CA", "kuhn.gamedef.xml",
                new IChanceAbstraction[] { new KuhnChanceAbstraction(), new KuhnChanceAbstraction() },
                0.001);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        [Test]
        public void Test_LeducHe_CA_Trivial()
        {
            // Take the same value as for exact game because this abstraction is losless.
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new TrivialChanceAbstraction(),
                    new TrivialChanceAbstraction(),
                },
                _isDebug ? 0.01 : 0.005);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        [Test]
        public void Test_LeducHe_CA_Fullgame()
        {
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                },
                _isDebug ? 0.01 : 0.005);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }


        [Test]
        public void Test_LeducHe_CA_Ocfr_FractionalResult()
        {
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                _isDebug ? 0.01 : 0.005);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        [Test]
        public void Test_LeducHe_CA_Ocfr_JQ_K_pair_nopair()
        {
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-JQK", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                },
                _isDebug ? 0.01 : 0.005);
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        /// <summary>
        /// Test one exact and one lossy abstraction.
        /// </summary>
        [Test]
        public void Test_LeducHe_CA_Ocfr_FullGame_FractionalResult()
        {
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FG-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                _isDebug ? 0.01 : 0.005);
            testParams.EqualCa = false;
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, null);
        }

        /// <summary>
        /// Test 2 different lossy abstractions.
        /// </summary>
        [Test]
        public void Test_LeducHe_CA_Ocfr__JQ_K_pair_nopair__FractionalResult()
        {
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-JQK-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                _isDebug ? 0.01 : 0.005);
            testParams.EqualCa = false;
            SolveAndVerifyVerifySolution(testParams, false, false, new int[] { 100, -1 }, s => 
              {
                  s.IsVerbose = true;
                  s.EpsilonLogThreshold = 0;
                  //s.IterationVerbosity = 1000;
                  //s.MaxIterationCount = 2000;
              });
        }

        #endregion

        #region Verification tests

        /// <summary>
        /// Makes sure that the strategies generated with an intermediate snapshot are exactly the same
        /// as without the snapshot.
        /// </summary>
        [Test]
        public void Test_Snapshot_Kuhn()
        {
            bool isVerbose = false;
            var testParams = new GameDefParams(this, "kuhn.gamedef.xml",
                0.001);
            testParams.Name = "Kunh-NoSh";
            StrategyTree[] trees = RunFictPlay(testParams, false, false, new int[] { -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            testParams.Name = "Kunh-Sh";
            StrategyTree[] treesSn = RunFictPlay(testParams, false, false, new int[] { 400, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            for (int p = 0; p < testParams.GameDef.MinPlayers; ++p)
            {
                CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
                cmp.Compare(trees[p], treesSn[p]);
                Assert.AreEqual(new double[] { 0, 0 }, cmp.SumProbabDiff);
            }
        }

        /// <summary>
        /// Makes sure that the strategies generated with an intermediate snapshot are exactly the same
        /// as without the snapshot.
        /// </summary>
        [Test]
        public void Test_Snapshot_MiniFl()
        {
            bool isVerbose = false;

            var testParams = new GameDefParams(this, "mini-fl.gamedef.xml",
                0.001);
            // Use a big iteration count for the snapshot, it is a good test for
            // initial setting of varless nodes.
            StrategyTree[] trees = RunFictPlay(testParams, false, false, new int[] { -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            StrategyTree[] treesSn = RunFictPlay(testParams, false, false, new int[] {10000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            for (int p = 0; p < testParams.GameDef.MinPlayers; ++p)
            {
                CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
                cmp.Compare(trees[p], treesSn[p]);
                Assert.AreEqual(new double[] {0, 0}, cmp.SumProbabDiff);
            }
        }

        /// <summary>
        /// Makes sure that the strategies generated with an intermediate snapshot are exactly the same
        /// as without the snapshot.
        /// </summary>
        [Test]
        public void Test_Snapshot_LeducHe()
        {
            bool isVerbose = true;

            var testParams = new GameDefParams(this, "leduc-he.gamedef.xml",
                0.002);
            StrategyTree[] trees = RunFictPlay(testParams, false, false, new int[] { -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            StrategyTree[] treesSn = RunFictPlay(testParams, false, false, new int[] { 10000, 20000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            for (int p = 0; p < testParams.GameDef.MinPlayers; ++p)
            {
                CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
                cmp.Compare(trees[p], treesSn[p]);
                Assert.AreEqual(new double[] { 0, 0 }, cmp.SumProbabDiff);
            }
        }

        /// <summary>
        /// Makes sure that the strategies generated with and without multithreading are exactly the same.
        /// </summary>
        [Test]
        public void Test_Multithreaded_LeducHe()
        {
            bool isVerbose = false;

            var testParams = new GameDefParams(this, "leduc-he.gamedef.xml",
                0.002);
            StrategyTree[] treesSt = RunFictPlay(testParams, false, false, new int[] { 10000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            StrategyTree[] treesMt = RunFictPlay(testParams, false, false, new int[] { 10000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 7; });
            for (int p = 0; p < testParams.GameDef.MinPlayers; ++p)
            {
                CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
                cmp.Compare(treesSt[p], treesMt[p]);
                Assert.AreEqual(new double[] { 0, 0 }, cmp.SumProbabDiff);
            }
        }

        #endregion


        #region Implementation

        static bool _isDebug;

        const int DEFAULT_THREADS_COUNT = 2;

        static FictitiousPlay_Test()
        {
#if DEBUG 
            _isDebug = true;
#else
            _isDebug = false;
#endif
        }

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "FictitiousPlay_Test");

        delegate void ConfigureSolver(FictitiousPlay solver);

        /// <summary>
        /// Base class for test parameters.
        /// </summary>
        class TestParams
        {
            public string Name;
            public ChanceTree ChanceTree;
            public ActionTree ActionTree;
            public double Epsilon;
            public bool EqualCa = true;
        }

        /// <summary>
        /// Test parameters to solve a game by game definition.
        /// The strategy trees are taken from XML and must correspond to the game.
        /// </summary>
        class GameDefParams : TestParams
        {
            public GameDefinition GameDef;

            public GameDefParams(FictitiousPlay_Test test, string gameDefFile, double epsilon)
            {
                Epsilon = epsilon;

                GameDef = XmlSerializerExt.Deserialize<GameDefinition>(Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}", gameDefFile));

                Name = GameDef.Name;

                ChanceTree = CreateChanceTreeByGameDef.Create(GameDef);
                ActionTree = CreateActionTreeByGameDef.Create(GameDef);
            }
        }

        /// <summary>
        /// Test parameters to solve an abstracted game.
        /// </summary>
        class ChanceAbstractedGameDefParams : GameDefParams
        {
            public ChanceAbstractedGameDefParams(FictitiousPlay_Test test, string name, string gameDefFile, 
                IChanceAbstraction[] abstractions, double epsilon) :
                base(test, gameDefFile, epsilon)
            {
                ChanceTree = CreateChanceTreeByAbstraction.CreateS(GameDef, abstractions);
                Name = name;
            }
        }

        /// <summary>
        /// Solves the game by fictitious play and verifies the solution.
        /// </summary>
        /// <param name="snapshotAfter">Number of iterations to make an intermediate snapshot after. -1 for no intermediate snapshot.</param>
        /// <param name="configureSolver"></param>
        private void SolveAndVerifyVerifySolution(TestParams testParams, bool visualize, bool trace, int[] iterCounts, ConfigureSolver configureSolver)
        {
            int playersCount = testParams.ChanceTree.PlayersCount;

            StrategyTree[] eqStrategies = RunFictPlay(testParams, visualize, trace, iterCounts, configureSolver);
            string error;

            // Verify consistency of strategies
            for(int p = 0; p < 2; ++p)
            {
                Assert.IsTrue(VerifyAbsStrategy.Verify(eqStrategies[p], p, 1e-7, out error), string.Format("Pos {0}: {1}", p, error));
            }

            // Run VerifyEq on the computed strategies. 
            Assert.IsTrue(VerifyEq.Verify(testParams.ActionTree, testParams.ChanceTree, 
                eqStrategies, 3*testParams.Epsilon, out error), error);

            //
            // Do a redundant test with EqLp
            //

            // Find game values for our solution
            GameValue gv = new GameValue
            {
                ActionTree = testParams.ActionTree,
                ChanceTree = testParams.ChanceTree,
                Strategies = eqStrategies
            };
            gv.Solve();

            // Solve eq with EqLp
            double[] expEqValues;
            StrategyTree[] expStrategies = EqLp.Solve(testParams.ActionTree, testParams.ChanceTree, out expEqValues);

            // Verify the eq value and strategy
            for (int p = 0; p < 2; ++p)
            {
                if (visualize)
                {
                    Console.WriteLine("Expected eq value pos {0}: {1}", p, expEqValues[p]);
                }
                Assert.AreEqual(expEqValues[p], gv.Values[p], testParams.Epsilon, "Eq value differs from EqLp solution");
            }

        }

        /// <summary>
        /// Runs FictitiousPlay with the specified parameters..
        /// </summary>
        /// <param name="iterCounts">Number of iterations for each run, -1 - unlimited.</param>
        private StrategyTree[] RunFictPlay(TestParams testParams, bool visualize, bool trace, int [] iterCounts, ConfigureSolver configureSolver)
        {
            int playersCount = testParams.ChanceTree.PlayersCount;

            string baseDir = Path.Combine(_outDir, testParams.Name);
            DirectoryExt.Delete(baseDir);
            Directory.CreateDirectory(baseDir);

            string inputDir = Path.Combine(baseDir, "input");
            Directory.CreateDirectory(inputDir);
            string traceDir = Path.Combine(baseDir, "trace");
            if (trace)
            {
                Directory.CreateDirectory(traceDir);
            }

            string chanceTreeFile = Path.Combine(inputDir, "ct.dat");
            string actionTreeFile = Path.Combine(inputDir, "at.dat");

            testParams.ChanceTree.Write(chanceTreeFile);
            testParams.ActionTree.Write(actionTreeFile);

            if (visualize)
            {
                VisActionTree.Show(testParams.ActionTree, actionTreeFile + ".gv");
                VisChanceTree.Show(testParams.ChanceTree, chanceTreeFile + ".gv");
            }


            int runs = iterCounts.Length;
            FictitiousPlay solver = null;
            for(int r = 0; r < runs; ++r)
            {
                // Create and configure a solver
                solver = new FictitiousPlay
                             {
                                 ChanceTreeFile = chanceTreeFile,
                                 EqualCa = testParams.EqualCa,
                                 ActionTreeFile = actionTreeFile,
                                 OutputPath = baseDir,
                                 SnapshotsCount = 2,
                                 Epsilon = testParams.Epsilon,
                                 ThreadsCount = DEFAULT_THREADS_COUNT
                             };
                if (trace)
                {
                    solver.TraceDir = traceDir;
                }
                solver.MaxIterationCount = iterCounts[r];
                if (configureSolver != null)
                {
                    configureSolver(solver);
                }
                solver.Solve();
            }

            StrategyTree[] eqStrategies = new StrategyTree[playersCount];

            for (int p = 0; p < playersCount; ++p)
            {
                string fileName = solver.CurrentSnapshotInfo.StrategyFile[p];
                eqStrategies[p] = StrategyTree.Read<StrategyTree>(fileName);
                if (visualize)
                {
                    VisStrategyTree.Show(eqStrategies[p], fileName + ".gv");
                }
            }
            return eqStrategies;
        }

        #endregion
    }
}
