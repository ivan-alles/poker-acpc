﻿/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ai.pkr.metagame;
using ai.lib.utils;
using System.Reflection;
using ai.pkr.metastrategy.nunit;
using System.Threading;
using System.Globalization;
using ai.pkr.metastrategy.vis;
using ai.lib.algorithms;
using ai.pkr.metastrategy.model_games;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for GameValue. 
    /// </summary>
    [TestFixture]
    public unsafe class GameValue_Test
    {
        #region Gamedef tests

        [Test]
        public void Test_Kuhn()
        {
            var testParams = new GameDefParams(this, "kuhn.gamedef.xml",
                new string[] { "eq-KunhPoker-0-s.xml", "eq-KunhPoker-1-s.xml" },
                new double[] { -1.0 / 18, 1.0 / 18 },
                0);
            // Set PrepareVis to true on purpose, to let it run too.
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = true;});
            //GameValue.Vis.Show(gv, 0, Path.Combine(_outDir, "KuhnPoker-values-0.gv"));
        }

        [Test]
        public void Test_Kuhn2()
        {
            var testParams = new GameDefParams(this, "kuhn2.gamedef.xml",
                new string[] { "eq-KunhPoker2-0-s.xml", "eq-KunhPoker2-1-s.xml" },
                new double[] { -1.0 / 18, 1.0 / 18 },
                0.00000000000000001);
            GameValue gv = Solve(testParams, false, null);
        }

        [Test]
        public void Test_Ocp()
        {
            var testParams = new GameDefParams(this, "ocp.gamedef.xml",
                new string[] { "eq-OneCardPoker-0-s.xml", "eq-OneCardPoker-1-s.xml" },
                new double[] { -0.0641025641026934, 0.0641025641026934 },
                0.0000000000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_Ocp2()
        {
            var testParams = new GameDefParams(this, "ocp2.gamedef.xml",
                new string[] { "eq-OneCardPoker2-0-s.xml", "eq-OneCardPoker2-1-s.xml" },
                new double[] { -0.0394736842106944, 0.0394736842106944 },
                0.0000000000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_LeducHe()
        {
            var testParams = new GameDefParams(this, "leduc-he.gamedef.xml",
                new string[] { "eq-LeducHe-0-s.xml", "eq-LeducHe-1-s.xml" },
                new double[] { -0.0428032120390257, 0.0428032120390257 },
                0.000000000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        /// <summary>
        /// This tests solves Leduc without abstraction.
        /// It uses FullGame abstraction of CFR as a reference strategy 
        /// because it corresponds to the non-abstracted game.
        /// </summary>
        [Test]
        public void Test_LeducHe_Ocfr_NoAbstr()
        {
            var testParams = new OcfrParams(this, "LeducHe", "leduc-he.gamedef.xml", 
                null,
                new string[] { "ocfr-FullGame-eq0.txt", "ocfr-FullGame-br1.txt" },
                new double[] { -0.0858505 / 2, 0.0858505 / 2 },
                0.000000005);
            //VisStrategyTree.Show(testParams.StrategyTrees[0], Path.Combine(_outDir, testParams.GameDef.Name+"-str-0.gv"));
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });

            testParams = new OcfrParams(this, "LeducHe", "leduc-he.gamedef.xml",
                null,
                new string[] { "ocfr-FullGame-br0.txt", "ocfr-FullGame-eq1.txt" },
                new double[] { -0.0854067 / 2, 0.0854067 / 2 },
                0.00000005);
            gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_LeducHeRb()
        {
            var testParams = new GameDefParams(this, "leduc-he-rb.gamedef.xml",
                new string[] { "eq-LeducHeRb-0-s.xml", "eq-LeducHeRb-1-s.xml" },
                new double[] { -0.00464121889644268, 0.00464121889644268 },
                0.000000000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_MicorFl()
        {
            var testParams = new GameDefParams(this, "micro-fl.gamedef.xml",
                new string[] { "eq-MicroFl-0-s.xml", "eq-MicroFl-1-s.xml" },
                new double[] { -1.0/18, 1.0/18},
                0.0000000000005);

            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_MiniFl()
        {
            var testParams = new GameDefParams(this, "mini-fl.gamedef.xml",
                new string[] { "eq-MiniFl-0-s.xml", "eq-MiniFl-1-s.xml" },
                new double[] { 0.0277777777778998, -0.0277777777778998 },
                0.0000000000005);
            GameValue gv = Solve(testParams, true, s => { s.PrepareVis = true; });
        }

        #endregion

        #region Abstract tests

        [Test]
        public void Test_Kuhn_CA()
        {
            ChanceAbstractedGameDefParams testParams = new ChanceAbstractedGameDefParams(this, "Kuhn-CA", "kuhn.gamedef.xml",
                new IChanceAbstraction []{ new KuhnChanceAbstraction(), new KuhnChanceAbstraction()},
                new string[] { "eq-KunhPoker-0-s.xml", "eq-KunhPoker-1-s.xml" },
                new double[] { -1.0 / 18, 1.0 / 18 },
                0);
            // Set PrepareVis to true on purpose, to let it run too.
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = true; });
        }

        /// <summary>
        /// Test a trivial abstraction, use FullGame as reference data because it corresponds to the 
        /// original game.
        /// </summary>
        [Test]
        public void Test_LeducHe_CA_Ocfr_Trivial()
        {
            var testParams = new OcfrParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new TrivialChanceAbstraction(),
                    new TrivialChanceAbstraction(),
                },
                new string[] { "ocfr-FullGame-eq0.txt", "ocfr-FullGame-br1.txt" },
                new double[] { -0.0858505 / 2, 0.0858505 / 2 },
                0.000000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });

            testParams = new OcfrParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new TrivialChanceAbstraction(),
                    new TrivialChanceAbstraction(),
                },
                new string[] { "ocfr-FullGame-br0.txt", "ocfr-FullGame-eq1.txt" },
                new double[] { -0.0854067 / 2, 0.0854067 / 2 },
                0.00000005);
            gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_LeducHe_CA_Ocfr_Fullgame()
        {
            var testParams = new OcfrParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                },
                new string[] { "ocfr-FullGame-eq0.txt", "ocfr-FullGame-br1.txt" },
                new double[] { -0.0858505 / 2, 0.0858505 / 2 },
                0.000000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });

            testParams = new OcfrParams(this, "LeducHe-FG", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                },
                new string[] { "ocfr-FullGame-br0.txt", "ocfr-FullGame-eq1.txt" },
                new double[] { -0.0854067 / 2, 0.0854067 / 2 },
                0.00000005);
            gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_LeducHe_CA_Ocfr_FractionalResult()
        {
            var testParams = new OcfrParams(this, "LeducHe-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new string[] { "ocfr-FractionalResult-eq0.txt", "ocfr-FractionalResult-br1.txt" },
                new double[] { -0.179122 / 2, 0.179122 / 2 },
                0.0000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });

            testParams = new OcfrParams(this, "LeducHe-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new string[] { "ocfr-FractionalResult-br0.txt", "ocfr-FractionalResult-eq1.txt" },
                new double[] { -0.17877 / 2, 0.17877 / 2 },
                0.0000005);
            gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        [Test]
        public void Test_LeducHe_CA_Ocfr_JQ_K_pair_nopair()
        {
            var testParams = new OcfrParams(this, "LeducHe-JQK", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                },
                new string[] { "ocfr-JQ_K_pair_nopair-eq0.txt", "ocfr-JQ_K_pair_nopair-br1.txt" },
                new double[] { -0.107589 / 2, 0.107589 / 2 },
                0.0000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });

            testParams = new OcfrParams(this, "LeducHe-JQK", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                },
                new string[] { "ocfr-JQ_K_pair_nopair-br0.txt", "ocfr-JQ_K_pair_nopair-eq1.txt" },
                new double[] { -0.107228 / 2, 0.107228 / 2 },
                0.0000005);
            gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        /// <summary>
        /// Test one exact and one lossy abstraction.
        /// </summary>
        [Test]
        public void Test_LeducHe_CA_Ocfr_FullGame_FractionalResult()
        {
            var testParams = new OcfrParams(this, "LeducHe-FG-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new string[] { "ocfr-FullGame-FractionalResult-eq0.txt", "ocfr-FullGame-FractionalResult-br1.txt" },
                new double[] { 0.215146 / 2, -0.215146 / 2 },
                0.0000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });

            testParams = new OcfrParams(this, "LeducHe-FG-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new string[] { "ocfr-FullGame-FractionalResult-br0.txt", "ocfr-FullGame-FractionalResult-eq1.txt" },
                new double[] { 0.215496 / 2, -0.215496 / 2 },
                0.0000005);
            gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }

        /// <summary>
        /// Test 2 different lossy abstractions.
        /// </summary>
        [Test]
        public void Test_LeducHe_CA_Ocfr__JQ_K_pair_nopair__FractionalResult()
        {
            var testParams = new OcfrParams(this, "LeducHe-JQK-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new string[] { "ocfr-JQ_K_pair_nopair-FractionalResult-eq0.txt", "ocfr-JQ_K_pair_nopair-FractionalResult-br1.txt" },
                new double[] { 0.0370542 / 2, -0.0370542 / 2 },
                0.0000005);
            GameValue gv = Solve(testParams, false, s => { s.PrepareVis = false; });

            testParams = new OcfrParams(this, "LeducHe-JQK-FR", "leduc-he.gamedef.xml",
                new IChanceAbstraction[] { 
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.JQ_K_pair_nopair),
                    new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),
                },
                new string[] { "ocfr-JQ_K_pair_nopair-FractionalResult-br0.txt", "ocfr-JQ_K_pair_nopair-FractionalResult-eq1.txt" },
                new double[] { 0.0375278 / 2, -0.0375278 / 2 },
                0.0000005);
            gv = Solve(testParams, false, s => { s.PrepareVis = false; });
        }


        #endregion


        #region Implementation

        delegate void ConfigureSolver(GameValue solver);

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "algorithms/GameValue_Test");

        /// <summary>
        /// Base class for test parameters.
        /// </summary>
        class TestParams
        {
            public string Name;
            public ChanceTree ChanceTree;
            public ActionTree ActionTree;
            public StrategyTree[] StrategyTrees;
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
            public string[] StrategyFiles;

            public GameDefParams(GameValue_Test test, string gameDefFile, string[] strategyFiles, double[] expectedResult, double epsilon)
            {
                Epsilon = epsilon;
                ExpectedResult = expectedResult;

                GameDef = XmlSerializerExt.Deserialize<GameDefinition>(
                    Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}",
                    gameDefFile));

                Name = GameDef.Name;

                StrategyFiles = new string[GameDef.MinPlayers].Fill(i => Path.Combine(test._testResDir, strategyFiles[i]));

                StrategyTrees = new StrategyTree[GameDef.MinPlayers].Fill(i =>
                    XmlToStrategyTree.Convert(StrategyFiles[i], GameDef.DeckDescr));

                ChanceTree = CreateChanceTreeByGameDef.Create(GameDef);
                ActionTree = CreateActionTreeByGameDef.Create(GameDef);
            }
        }

        /// <summary>
        /// Test parameters to solve an abstracted game.
        /// The strategy trees are taken from XML and must correspond to the abstracted game.
        /// </summary>
        class ChanceAbstractedGameDefParams : GameDefParams
        {
            public ChanceAbstractedGameDefParams(GameValue_Test test, string name, string gameDefFile, IChanceAbstraction[] abstractions,
                string[] strategyFiles, double[] expectedResult, double epsilon) : 
                base(test, gameDefFile, strategyFiles, expectedResult, epsilon)
            {
                ChanceTree = CreateChanceTreeByAbstraction.CreateS(GameDef, abstractions);
                Name = name;
            }
        }

        /// <summary>
        /// Test parameters to solve a game from Open CFR files.
        /// </summary>
        class OcfrParams : TestParams
        {
            public GameDefinition GameDef;
            public string[] OcfrFiles;

            public OcfrParams(GameValue_Test test, string name, string gameDefFile, IChanceAbstraction[] abstractions, string[] ocfrFiles, double[] expectedResult, double epsilon)
            {
                Epsilon = epsilon;
                ExpectedResult = expectedResult;

                GameDef = XmlSerializerExt.Deserialize<GameDefinition>(
                    Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}",
                    gameDefFile));

                Name = name;

                OcfrFiles = new string[GameDef.MinPlayers].Fill(i => Path.Combine(test._testResDir, ocfrFiles[i]));
                if (abstractions == null)
                {
                    ChanceTree = CreateChanceTreeByGameDef.Create(GameDef);
                }
                else
                {
                    ChanceTree = CreateChanceTreeByAbstraction.CreateS(GameDef, abstractions);
                }

                //VisChanceTree.Show(ChanceTree, Path.Combine(test._outDir, String.Format("{0}-c.gv", GameDef.Name)));

                ActionTree = CreateActionTreeByGameDef.Create(GameDef);

                StrategyTrees = new StrategyTree[GameDef.MinPlayers];

                for(int p = 0; p < GameDef.MinPlayers; ++p)
                {
                    ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ChanceTree, p);
                    StrategyTrees[p] = CreateStrategyTreeByChanceAndActionTrees.CreateS(pct, ActionTree);


                    OpenCfrStrategyConverter conv = new OpenCfrStrategyConverter
                    {
                        GameDef = GameDef,
                        HeroPosition = p,
                        SourceFile = OcfrFiles[p],
                        Strategy = StrategyTrees[p],
                        ChanceAbstraction = abstractions == null ? null : abstractions[p]
                    };
                    conv.Convert();

                    //VisStrategyTree.Show(StrategyTrees[p], Path.Combine(test._outDir, String.Format("{0}-str-{1}.gv", GameDef.Name, p)));
                }
            }
        }

        private GameValue Solve(TestParams testParams, bool visualize, ConfigureSolver configureSolver)
        {
            if (visualize)
            {
                VisActionTree.Show(testParams.ActionTree,
                                   Path.Combine(_outDir, String.Format("{0}-at.gv", testParams.Name)));
                VisChanceTree.Show(testParams.ChanceTree,
                                   Path.Combine(_outDir, String.Format("{0}-ct.gv", testParams.Name)));
                for (int p = 0; p < testParams.StrategyTrees.Length; ++p)
                {
                    VisStrategyTree.Show(testParams.StrategyTrees[p],
                                         Path.Combine(_outDir, string.Format("{0}-st-{1}.gv", testParams.Name, p)));
                }
            }

            // Make sure input is correct.
            for (int p = 0; p < testParams.ChanceTree.Nodes[0].Position; ++p)
            {
                string errorText;
                Assert.IsTrue(VerifyAbsStrategy.Verify(testParams.StrategyTrees[p], p, 0.000001, out errorText), errorText);
            }

            GameValue gv = new GameValue { ChanceTree = testParams.ChanceTree, ActionTree = testParams.ActionTree, Strategies = testParams.StrategyTrees };
            gv.PrepareVis = visualize;

            if (configureSolver != null)
            {
                configureSolver(gv);
            }

            gv.Solve();

            if (visualize)
            {
                for (int p = 0; p < testParams.ChanceTree.PlayersCount; ++p)
                {
                    GameValue.Vis.Show(gv, p, Path.Combine(_outDir, String.Format("{0}-{1}-val.gv", testParams.Name, p)));
                }
            }

            Assert.AreEqual(2, gv.Values.Length);
            for (int p = 0; p < testParams.ChanceTree.PlayersCount; ++p)
            {
                Console.WriteLine("Game value pos {0}: {1}", p, gv.Values[p]);
                Assert.AreEqual(testParams.ExpectedResult[p], gv.Values[p], testParams.Epsilon);
            }
            return gv;
        }

        #endregion
    }
}
