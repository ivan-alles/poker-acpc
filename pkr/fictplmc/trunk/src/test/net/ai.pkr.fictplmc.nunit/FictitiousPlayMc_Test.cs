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
using System.Diagnostics;

namespace ai.pkr.fictpl.nunit
{
    /// <summary>
    /// Unit tests for FictitiousPlayMc. 
    /// </summary>
    [TestFixture]
    public class FictitiousPlayMc_Test
    {
        public FictitiousPlayMc_Test()
        {
            //Console.WriteLine("Turn on unmanaged memory diagnostics");
            //UnmanagedMemory.IsDiagOn = true;
        }

        #region Tests for subclasses

        #endregion

        #region Solver tests

        [Test]
        public void Test_Kuhn()
        {
            TestParams testParams = new TestParams(this, "kuhn.gamedef.xml",
                new KuhnChanceAbstraction(), new int[]{3}, 0.001);
            SolveAndVerifyVerifySolution(testParams, true, true,
                new int[] { -1 },
                s =>
                  {
                      s.IsVerbose = true;
                      s.EpsilonLogThreshold = 0;
                      s.IterationVerbosity = 1000;
                      s.MaxIterationCount = 20;
                      s.ThreadsCount = 0; // Test single-threaded variant.
                  });
        }

        /*
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
        */
/*
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

 * */
        #endregion

        #region Verification tests
        /*
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
            StrategyTree eqStrategy = RunFictPlay(testParams, false, false, new int[] { -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            testParams.Name = "Kunh-Sh";
            StrategyTree eqStrategySn = RunFictPlay(testParams, false, false, new int[] { 400, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
            cmp.Compare(eqStrategy, eqStrategySn);
            Assert.AreEqual(new double[] { 0, 0 }, cmp.SumProbabDiff);
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
            StrategyTree eqStrategy = RunFictPlay(testParams, false, false, new int[] { -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            StrategyTree eqStrategySn = RunFictPlay(testParams, false, false, new int[] { 10000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
            cmp.Compare(eqStrategy, eqStrategySn);
            Assert.AreEqual(new double[] {0, 0}, cmp.SumProbabDiff);
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
            StrategyTree eqStrategy = RunFictPlay(testParams, false, false, new int[] { -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            StrategyTree eqStrategySn = RunFictPlay(testParams, false, false, new int[] { 10000, 20000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
            cmp.Compare(eqStrategy, eqStrategySn);
            Assert.AreEqual(new double[] { 0, 0 }, cmp.SumProbabDiff);
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
            StrategyTree treeSt = RunFictPlay(testParams, false, false, new int[] { 10000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 0; });
            StrategyTree treeMt = RunFictPlay(testParams, false, false, new int[] { 10000, -1 },
                s => { s.IsVerbose = isVerbose; s.ThreadsCount = 7; });
            CompareStrategyTrees cmp = new CompareStrategyTrees { IsVerbose = isVerbose };
            cmp.Compare(treeSt, treeMt);
            Assert.AreEqual(new double[] { 0, 0 }, cmp.SumProbabDiff);
        }
        */
        #endregion


        #region Implementation

        static bool _isDebug;

        const int DEFAULT_THREADS_COUNT = 2;

        static FictitiousPlayMc_Test()
        {
#if DEBUG 
            _isDebug = true;
#else
            _isDebug = false;
#endif
        }

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "FictitiousPlay_Test");

        delegate void ConfigureSolver(FictitiousPlayMc solver);

        /// <summary>
        /// Base class for test parameters.
        /// </summary>
        class TestParams
        {
            public string Name;
            /// <summary>
            /// Chance tree. Is not used by the solver, but is necessary for verification.
            /// </summary>
            public ChanceTree ChanceTree;
            public ActionTree ActionTree;
            public double Epsilon;
            public int [] CardCount;
            public GameDefinition GameDef;
            public IChanceAbstraction ChanceAbstraction;

            public TestParams(FictitiousPlayMc_Test test, string gameDefFile, 
                IChanceAbstraction chanceAbstr, int [] cardCount, double epsilon) 
            {
                Epsilon = epsilon;
                GameDef = XmlSerializerExt.Deserialize<GameDefinition>(Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}", gameDefFile));
                Name = chanceAbstr.Name;
                ChanceAbstraction = chanceAbstr;

                ActionTree = CreateActionTreeByGameDef.Create(GameDef);
                Debug.Assert(GameDef.MinPlayers == 2);
                ChanceTree = CreateChanceTreeByAbstraction.CreateS(GameDef, new IChanceAbstraction[]{ chanceAbstr, chanceAbstr});
                CardCount = cardCount;
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

            StrategyTree eqStrategy = RunFictPlay(testParams, visualize, trace, iterCounts, configureSolver);
            string error;

            // Verify consistency of strategies
            for(int p = 0; p < 2; ++p)
            {
                Assert.IsTrue(VerifyAbsStrategy.Verify(eqStrategy, p, 1e-7, out error), string.Format("Pos {0}: {1}", p, error));
            }

            // Run VerifyEq on the computed strategies. 
            StrategyTree[] strategies = new StrategyTree[] {eqStrategy, eqStrategy };
            Assert.IsTrue(VerifyEq.Verify(testParams.ActionTree, testParams.ChanceTree,
                strategies, 3 * testParams.Epsilon, out error), error);

            //
            // Do a redundant test with EqLp
            //

            // Find game values for our solution
            GameValue gv = new GameValue
            {
                ActionTree = testParams.ActionTree,
                ChanceTree = testParams.ChanceTree,
                Strategies = new StrategyTree[] {eqStrategy, eqStrategy}
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
        private StrategyTree RunFictPlay(TestParams testParams, bool visualize, bool trace, int [] iterCounts, ConfigureSolver configureSolver)
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
            FictitiousPlayMc solver = null;
            for(int r = 0; r < runs; ++r)
            {
                // Create and configure a solver
                solver = new FictitiousPlayMc
                             {
                                 GameDef = testParams.GameDef,
                                 ChanceAbstraction = testParams.ChanceAbstraction,
                                 ActionTreeFile = actionTreeFile,
                                 OutputPath = baseDir,
                                 SnapshotsCount = 2,
                                 Epsilon = testParams.Epsilon,
                                 ThreadsCount = DEFAULT_THREADS_COUNT,
                                 CardCount = testParams.CardCount
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

            string fileName = solver.CurrentSnapshotInfo.StrategyFile;
            StrategyTree eqStrategy = StrategyTree.Read<StrategyTree>(fileName);
            if (visualize)
            {
                VisStrategyTree.Show(eqStrategy, fileName + ".gv");
            }
            return eqStrategy;
        }

        #endregion
    }
}
