/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy.algorithms;
using System.Reflection;
using System.IO;
using ai.pkr.metastrategy;
using ai.lib.algorithms.tree;
using ai.lib.algorithms;
using ai.pkr.metastrategy.nunit;
using ai.pkr.metastrategy.vis;
using ai.pkr.metastrategy.model_games;
using lpsolve55;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for EqLp. 
    /// </summary>
    [TestFixture]
    public unsafe class EqLp_Test
    {
        #region lpsolve test
        [Test]
        public void Test_lpsolver_API()
        {
            string pathToDll = Props.Global.Expand("${bds.BinDir}win32");
            Assert.IsTrue(lpsolve.Init(pathToDll));
        }
        #endregion

        #region Gamedef tests

        [Test]
        public void Test_Kuhn()
        {
            var testParams = new GameDefParams(this, "kuhn.gamedef.xml",
                new double[] { -1.0 / 18, 1.0 / 18 },
                0.0000000000005);
            Solve(testParams, false, null);
            //solver.PrintCPLEX(Console.Out);
        }
        [Test]
        public void Test_Kuhn2()
        {
            var testParams = new GameDefParams(this, "kuhn2.gamedef.xml",
                new double[] { -1.0 / 18, 1.0 / 18 },
                0.0000000000005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_Ocp()
        {
            var testParams = new GameDefParams(this, "ocp.gamedef.xml",
                new double[] { -0.0641025641026934, 0.0641025641026934 },
                0.000000000005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_Ocp2()
        {
            var testParams = new GameDefParams(this, "ocp2.gamedef.xml",
                new double[] { -0.0394736842106944, 0.0394736842106944 },
                0.000000000005);
            Solve(testParams, false, null);
        }


        [Test]
        public void Test_LeducHe()
        {
            var testParams = new GameDefParams(this, "leduc-he.gamedef.xml",
                new double[] { -0.0428032120390257, 0.0428032120390257 },
                0.000000000005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_LeducHeRb()
        {
            var testParams = new GameDefParams(this, "leduc-he-rb.gamedef.xml",
                new double[] { -0.00464121889644268, 0.00464121889644268 },
                0.000000000005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_MicorFl()
        {
            var testParams = new GameDefParams(this, "micro-fl.gamedef.xml",
                new double[] { -1.0 / 18, 1.0 / 18 },
                0.000000000005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_MiniFl()
        {
            var testParams = new GameDefParams(this, "mini-fl.gamedef.xml",
                new double[] { 0.0277777777778998, -0.0277777777778998 },
                0.000000000005);
            Solve(testParams, false, null);
        }

        #endregion

        #region Abstract tests


        [Test]
        public void Test_Kuhn_CA()
        {
            ChanceAbstractedGameDefParams testParams = new ChanceAbstractedGameDefParams(this, "Kuhn-CA", "kuhn.gamedef.xml",
                new IChanceAbstraction[] { new KuhnChanceAbstraction(), new KuhnChanceAbstraction() },
                new double[] { -1.0 / 18, 1.0 / 18 },
                0.0000000000005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_LeducHe_CA_Trivial()
        {
            // Take the same value as for exact game because this abstraction is losless.
            double expValue = 0.0428032120390257;
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new TrivialChanceAbstraction(),
                    new TrivialChanceAbstraction(),
                },
                new double[] { -expValue, expValue }, 0.0000000000005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_LeducHe_CA_Fullgame()
        {
            // Take the same value as for exact game because this abstraction is losless.
            double expValue = 0.0428032120390257;
            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                },
                new double[] { -expValue, expValue }, 0.0000000000005);
            Solve(testParams, false, null);
        }


        [Test]
        public void Test_LeducHe_CA_Ocfr_FractionalResult()
        {
            // Take average from 2 OCFR BRs and divide by 2 for our stakes.
            double expValue = ( 0.17877 + 0.179122 ) / 4;

            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new double[] { -expValue, expValue },
                0.0005);
            Solve(testParams, false, null);
        }

        [Test]
        public void Test_LeducHe_CA_Ocfr_JQ_K_pair_nopair()
        {
            // Take average from 2 OCFR BRs and divide by 2 for our stakes.
            double expValue = ( 0.107228 + 0.107589 ) / 4;

            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-JQK", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                },
                new double[] { -expValue, expValue },
                0.00005);
            Solve(testParams, false, null);
        }

        /// <summary>
        /// Test one exact and one lossy abstraction.
        /// </summary>
        [Test]
        public void Test_LeducHe_CA_Ocfr_FullGame_FractionalResult()
        {
            // Take average from 2 OCFR BRs and divide by 2 for our stakes.
            double expValue = ( 0.215496 + 0.215146  ) / 4;

            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-FG-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new double[] { expValue, -expValue },
                0.00005);
            Solve(testParams, false, null);
        }

        /// <summary>
        /// Test 2 different lossy abstractions.
        /// </summary>
        [Test]
        public void Test_LeducHe_CA_Ocfr__JQ_K_pair_nopair__FractionalResult()
        {
            // Take average from 2 OCFR BRs and divide by 2 for our stakes.
            double expValue = ( 0.0375278 + 0.0370542 ) / 4;

            var testParams = new ChanceAbstractedGameDefParams(this, "LeducHe-JQK-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new double[] { expValue, -expValue },
                0.00025);
            Solve(testParams, false, null);
        }

        #endregion

        #region Implementation

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "algorithms/EqLp_Test");

        delegate void ConfigureSolver(EqLp solver);

        /// <summary>
        /// Base class for test parameters.
        /// </summary>
        class TestParams
        {
            public string Name;
            public ChanceTree ChanceTree;
            public ActionTree ActionTree;
            public double[] ExpectedResult;
            public double Epsilon;
        }

        /// <summary>
        /// Test parameters to solve a game by game definition.
        /// The strategy trees are taken from XML and must correspond to the game.
        /// </summary>
        class GameDefParams : TestParams
        {
            public GameDefinition GameDef;

            public GameDefParams(EqLp_Test test, string gameDefFile, double[] expectedResult, double epsilon)
            {
                Epsilon = epsilon;
                ExpectedResult = expectedResult;

                GameDef = XmlSerializerExt.Deserialize<GameDefinition>(
                    Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}",
                    gameDefFile));

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
            public ChanceAbstractedGameDefParams(EqLp_Test test, string name, string gameDefFile, IChanceAbstraction[] abstractions,
                double[] expectedResult, double epsilon) :
                base(test, gameDefFile, expectedResult, epsilon)
            {
                ChanceTree = CreateChanceTreeByAbstraction.CreateS(GameDef, abstractions);
                Name = name;
            }
        }

        private void Solve(TestParams testParams, bool visualize, ConfigureSolver configureSolver)
        {
            if (visualize)
            {
                VisActionTree.Show(testParams.ActionTree,
                                   Path.Combine(_outDir, String.Format("{0}-at.gv", testParams.Name)));
                VisChanceTree.Show(testParams.ChanceTree,
                                   Path.Combine(_outDir, String.Format("{0}-ct.gv", testParams.Name)));
            }

            StrategyTree [] eqStrategies = new StrategyTree[testParams.ChanceTree.PlayersCount];

            string error;

            for (int heroPos = 0; heroPos < testParams.ChanceTree.PlayersCount; ++heroPos)
            {
                // Create and configure EqLp solver
                EqLp solver = new EqLp
                            {
                                HeroPosition = heroPos,
                                ChanceTree = testParams.ChanceTree,
                                ActionTree = testParams.ActionTree,
                            };
                
                if (configureSolver != null)
                {
                    configureSolver(solver);
                }

                // Solve EqLp
                solver.Solve();
                eqStrategies[heroPos] = solver.Strategy;

                if (visualize)
                {
                    VisStrategyTree.Show(solver.Strategy,
                                         Path.Combine(_outDir, string.Format("{0}-eq-{1}.gv", testParams.Name, heroPos)));

                }

                // Verify the eq value and strategy
                Assert.AreEqual(testParams.ExpectedResult[heroPos], solver.Value, testParams.Epsilon, "Wrong eq value");
                Assert.IsTrue(VerifyAbsStrategy.Verify(solver.Strategy, solver.HeroPosition, 1e-7, out error), error);
            }
            // Verify eq, use another (better) epsilon because EqLp and VerifyEq have better precision
            // than most of the reference game solvers like OCFR.
            Assert.IsTrue(VerifyEq.Verify(testParams.ActionTree, testParams.ChanceTree, eqStrategies, 1e-7, out error), error);
        }


        #endregion
    }
}
