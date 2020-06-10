/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using System.Reflection;
using System.IO;
using ai.pkr.metabots;
using ai.pkr.metagame;
using ai.pkr.metatools;
using ai.lib.algorithms.numbers;
using ai.lib.algorithms.tree;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metastrategy.model_games;
using ai.pkr.metastrategy.vis;

namespace ai.pkr.bots.patience.nunit
{
    /// <summary>
    /// Unit tests for Patience. 
    /// </summary>
    [TestFixture]
    public class Patience_Test
    {
        #region Tests

        private const int LEDUC_ALL_GAMES_LOG_SIZE = 240;

        /// <summary>
        /// Play a low number of games with a huge epsilon to quickly see if everything is running.
        /// </summary>
        [Test]
        public void Test_EqVsBr_Leduc_Public_Quick()
        {
            int repeatCount = 100;
            double relEpsion = 1;
            string[] bucketStrings = new string[] { LeducHeChanceAbstraction.Public, LeducHeChanceAbstraction.Public };
            PlayEqVsBr(bucketStrings, LEDUC_ALL_GAMES_LOG_SIZE, repeatCount, relEpsion);
        }

        /// <summary>
        /// Play 2 identical exact abstractions.
        /// </summary>
        [Test]
        [Category("LongRunning")]
        public void Test_EqVsBr_Leduc_Public()
        {
#if DEBUG
            int repeatCount = 100;
            double relEpsion = 1;
#else
            int repeatCount = 3000;
            double relEpsion = 0.06;
#endif
            string[] bucketStrings = new string[] { LeducHeChanceAbstraction.Public, LeducHeChanceAbstraction.Public };
            PlayEqVsBr(bucketStrings, LEDUC_ALL_GAMES_LOG_SIZE, repeatCount, relEpsion);
        }

        /// <summary>
        /// Play one exact and 1 lossy abstractions with a paradoxal result (positive result in pos 0).
        /// </summary>
        [Test]
        [Category("LongRunning")]
        public void Test_EqVsBr_Leduc_FullGame_FractionalResult()
        {
#if DEBUG
            int repeatCount = 100;
            double relEpsion = 1;
#else
            int repeatCount = 2000;
            double relEpsion = 0.03;
#endif
            string[] bucketStrings = new string[] { LeducHeChanceAbstraction.FullGame, LeducHeChanceAbstraction.FractionalResult };
            PlayEqVsBr(bucketStrings, LEDUC_ALL_GAMES_LOG_SIZE, repeatCount, relEpsion);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _testResourceDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        private string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "Patience_Test");
        private TotalResult _actualResult;


        /// <summary>
        /// Plays the following scenario:
        /// 1. Prepares chance abstractions.
        /// 2. Computes a eq-strategy by EqLp.
        /// 3. Computes a BR on the eq strategies.
        /// 4. Runs a session with 2 instances of Patience, one is playing eq, the other br.
        /// 5. Compares actual game result with the predicted game value.
        /// The test is trying to reuse chance abstractions and merge strategies for different positions 
        /// if the abstraction is the same.
        /// <param name="baseDir">The function copies all config files from _testResourceDir/baseDir
        /// to _outDir/baseDir-eq and _outDir/baseDir-br, all intermediate files are also created here.</param>
        /// </summary>
        void PlayEqVsBr(string [] bucketizerStrings, int sessionGamesCount, int sessionRepetitionCount, double relativeTolerance)
        {
            Console.WriteLine("Run eq vs eq for chance abstractions:");
            for (int p = 0; p < bucketizerStrings.Length; ++p)
            {
                Console.WriteLine("pos: {0} bucket string: {1}", p, bucketizerStrings[0]);
            }

            GameDefinition gd = XmlSerializerExt.Deserialize<GameDefinition>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}", "leduc-he.gamedef.xml"));

            string runDir = PrepareRunDir("EqVsBr_Leduc");
            IChanceAbstraction [] chanceAbstractions = PrepareConfigsAndChanceAbstractions(runDir, bucketizerStrings);
            double [] brValues = new double[gd.MinPlayers];
            SolveGame(runDir, gd, chanceAbstractions, brValues);

            Console.WriteLine("Values of BR strategies:");
            for (int p = 0; p < brValues.Length; ++p)
            {
                Console.WriteLine("pos: {0} val: {1:0.00} mb0/g", p, brValues[p] * 1000);
            }

            // To tell the bots where the configs are:
            Props.Global.Set("dev.TestRunDir", runDir);

            runDir = CopyDirForEachBot(runDir, gd.MinPlayers);
            // Now runDir is eq dir.

            _actualResult = new TotalResult{Name = "Actual result"};

            string ssConfigFile = Path.Combine(runDir, "ss.xml");
            SessionSuiteRunner runner = new SessionSuiteRunner();
            runner.Configuration = XmlSerializerExt.Deserialize<SessionSuiteCfg>(ssConfigFile);
            runner.Configuration.Sessions[0].GamesCount = sessionGamesCount;
            runner.Configuration.Sessions[0].RepeatCount = sessionRepetitionCount;
            runner.IsLoggingEnabled = false;
            runner.OnGameEnd += new SessionSuiteRunner.OnGameEndHandler(runner_OnGameEnd);
            runner.Run();

            _actualResult.Print(Console.Out);

            for (int p = 0; p < gd.MinPlayers; ++p)
            {
                double expectedResult = brValues[p] * 1000;
                double actualResult = _actualResult.Players["Patience-Br"].Rate(p);
                double epsilon = Math.Abs(expectedResult * relativeTolerance);
                Assert.AreEqual(expectedResult, actualResult, epsilon);
            }

            // We can use the files created in other tests.
            //DeleteBotDirs(runDir);
        }

        void runner_OnGameEnd(GameRecord gameRecord)
        {
            _actualResult.Update(gameRecord);
        }

        void DeleteBotDirs(string runDir)
        {
            DirectoryExt.Delete(runDir + "-br");
            DirectoryExt.Delete(runDir + "-eq");
        }


        string CopyDirForEachBot(string runDir, int playerCount)
        {
            // Start from scratch
            DeleteBotDirs(runDir);

            string dirName = runDir + "-br";
            DirectoryExt.Copy(runDir, dirName);
            // For br: rename: strategy-{#}-br.dat -> strategy-{#}.dat
            for (int p = 0; p < playerCount; ++p)
            {
                
                string fileName = Path.Combine(dirName, string.Format("strategy-{0}.dat", p));
                // Check if this strategy was merged, if yes, skip it.
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    string fileName1 = Path.Combine(dirName, string.Format("strategy-{0}-br.dat", p));
                    File.Move(fileName1, fileName);
                }
            }

            dirName = runDir + "-eq";
            Directory.Move(runDir, dirName);
            // For eq: remove br files
            for (int p = 0; p < playerCount; ++p)
            {
                string fileName = Path.Combine(dirName, string.Format("strategy-{0}-br.dat", p));
                // Check if this strategy was merged, if yes, skip it.
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }

            return dirName;
        }

        /// <summary>
        /// Creates files for each chance abstraction based on the template file.
        /// Tries to reuse the same chance abstration if buckets strings are the same 
        /// to test Patience in this mode.
        /// </summary>
        /// <param name="runDir"></param>
        /// <param name="bucketizerStrings"></param>
        /// <returns></returns>
        private IChanceAbstraction[] PrepareConfigsAndChanceAbstractions(string runDir, string[] bucketizerStrings)
        {
            string botPropsFile = Path.Combine(runDir, "props.xml");
            Props botProps = XmlSerializerExt.Deserialize<Props>(botPropsFile);

            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[bucketizerStrings.Length];

            string caPropsTemplateFile = Path.Combine(runDir, "chance-abstraction-props-template.xml");

            for (int p = 0; p < bucketizerStrings.Length; ++p)
            {
                for (int p1 = 0; p1 < p; ++p1)
                {
                    // Try to reuse the same chance abstraction.
                    if (bucketizerStrings[p1] == bucketizerStrings[p])
                    {
                        botProps.Set(string.Format("ChanceAbstraction-{0}", p),
                                     string.Format("chance-abstraction-props-{0}.xml", p1));
                        botProps.Set(string.Format("Strategy-{0}", p), string.Format("strategy-{0}.dat", p1));
                        chanceAbstractions[p] = chanceAbstractions[p1];
                        goto next;
                    }
                }

                Props caProps = XmlSerializerExt.Deserialize<Props>(caPropsTemplateFile);
                caProps.Set("BucketsString", bucketizerStrings[p]);
                botProps.Set(string.Format("ChanceAbstraction-{0}", p),
                             string.Format("chance-abstraction-props-{0}.xml", p));

                string caPropsFileRel = string.Format("chance-abstraction-props-{0}.xml", p);
                string caPropsFileAbs = Path.Combine(runDir, caPropsFileRel);
                XmlSerializerExt.Serialize(caProps, caPropsFileAbs);
                botProps.Set(string.Format("ChanceAbstraction-{0}", p), caPropsFileRel);
                botProps.Set(string.Format("Strategy-{0}", p), string.Format("strategy-{0}.dat", p));

                chanceAbstractions[p] = ChanceAbstractionHelper.CreateFromProps(caProps);

            next: ;
            }
            XmlSerializerExt.Serialize(botProps, botPropsFile);
            File.Delete(caPropsTemplateFile);
            return chanceAbstractions;
        }

        void SolveGame(string runDir, GameDefinition gd, IChanceAbstraction [] chanceAbstractions, double [] brValues)
        {
            ChanceTree ct = CreateChanceTreeByAbstraction.CreateS(gd, chanceAbstractions);
            ActionTree at = CreateActionTreeByGameDef.Create(gd);

            double[] eqValues;
            StrategyTree[] eqStrategies = EqLp.Solve(at, ct, out eqValues);

            string error;

            for (int p = 0; p < gd.MinPlayers; ++p)
            {
                Assert.IsTrue(VerifyAbsStrategy.Verify(eqStrategies[p], p, 1e-7, out error), error);
            }
            // Verify eq
            Assert.IsTrue(VerifyEq.Verify(at, ct, eqStrategies, 1e-7, out error), error);

            StrategyTree[] brStrategies = new StrategyTree[gd.MinPlayers];

            // Find BR for each position
            for (int heroPos = 0; heroPos < gd.MinPlayers; ++heroPos)
            {

                Br br = new Br
                {
                    HeroPosition = heroPos,
                    ActionTree = at, 
                    ChanceTree = ct, 
                    Strategies = (StrategyTree[])eqStrategies.Clone()
                };
                br.Solve();
                brStrategies[heroPos] = br.Strategies[heroPos];
                brValues[heroPos] = br.Value;
            }

            MergeAndSaveStrategies(runDir, "", eqStrategies, chanceAbstractions);
            MergeAndSaveStrategies(runDir, "-br", brStrategies, chanceAbstractions);
        }

        void MergeAndSaveStrategies(string runDir, string suffix, StrategyTree[] strategies, IChanceAbstraction[] chanceAbstractions)
        {
            for (int p = 1; p < strategies.Length; ++p)
            {
                for (int p1 = 0; p1 < p; ++p1)
                {
                    if (chanceAbstractions[p1] == chanceAbstractions[p])
                    {
                        MergeStrategies.Merge(strategies[p1], strategies[p], p);
                        strategies[p] = null;
                    }
                }
            }
            for (int p = 0; p < strategies.Length; ++p)
            {
                if (strategies[p] == null) continue;
                string fileName = Path.Combine(runDir, string.Format("strategy-{0}{1}.dat", p, suffix));
                strategies[p].Write(fileName);
                //VisStrategyTree.Show(strategies[p], fileName + ".gv");
            }
        }


        private string PrepareRunDir(string baseDir)
        {
            string configDir = Path.Combine(_testResourceDir, baseDir);
            string runDir = Path.Combine(_outDir, baseDir);

            DirectoryExt.Delete(runDir);
            DirectoryExt.Copy(configDir, runDir);
            return runDir;
        }

        

        #endregion
    }
}
