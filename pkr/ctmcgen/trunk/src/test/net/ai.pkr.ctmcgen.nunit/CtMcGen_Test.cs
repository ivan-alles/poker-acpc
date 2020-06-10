/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metastrategy;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.pkr.metastrategy.model_games;
using System.IO;
using ai.pkr.metastrategy.vis;
using System.Reflection;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.ctmcgen.nunit
{
    /// <summary>
    /// Unit tests for CtMcGen. 
    /// </summary>
    /// Todo: clean-up and refactoring.
    [TestFixture]
    public class CtMcGen_Test
    {
        #region Tests

        /// <summary>
        /// Tests Read() and Write():
        /// 1. Creates an intermediate tree and converts it to the chance tree.
        /// 2. Writes the intermediate tree, reads it again and converts to the 2nd chance tree.
        /// 3. Compares public data of the intermediate trees.
        /// 4. compares the chance trees.
        /// </summary>
        [Test]
        public void Test_ReadWrite()
        {
            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[]
            {
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),    
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult)
            };

            CtMcGen.Tree tree1 = CtMcGen.Generate(_leducHeGd, chanceAbstractions, false, 10000, (int)DateTime.Now.Ticks, null);
            ChanceTree ct1 = tree1.ConvertToChanceTree();

            string fileName = Path.Combine(_outDir, "ctmcgen-tree.dat");
            tree1.Write(fileName);
            CtMcGen.Tree tree2 = new CtMcGen.Tree();
            tree2.Read(fileName);

            // Compare public data
            Assert.AreEqual(tree1.CalculateLeavesCount(), tree2.CalculateLeavesCount());
            Assert.AreEqual(tree1.SamplesCount, tree2.SamplesCount);
            Assert.AreEqual(tree1.Version, tree2.Version);

            ChanceTree ct2 = tree2.ConvertToChanceTree();

            // Compare two chance trees, they must be exactly the same.
            CompareChanceTrees cmp = new CompareChanceTrees();
            cmp.Compare(ct1, ct2);
            Assert.AreEqual(0, cmp.SumProbabDiff);
            for (int p = 0; p < chanceAbstractions.Length; ++p)
            {
                Assert.AreEqual(0, cmp.SumPotShareDiff[p]);
            }
        }

        /// <summary>
        /// Tests MergeWith().
        /// Creates 2 trees with the same MC sample counts and RNG seed, merges 2nd into 1st.
        /// Then compares chance trees, they must be the same.
        /// It is difficult to verify more, therefore merging is tested indirectly in MC generation tests.
        /// </summary>
        [Test]
        public void Test_Merge()
        {
            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[]
            {
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),    
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult)
            };

            CtMcGen.Tree tree1 = CtMcGen.Generate(_leducHeGd, chanceAbstractions, false, 10000, 333, null);
            CtMcGen.Tree tree2 = CtMcGen.Generate(_leducHeGd, chanceAbstractions, false, 10000, 333, null);

            UInt64 expSamplesCount = tree1.SamplesCount + tree2.SamplesCount;

            string fileName = Path.Combine(_outDir, "ctmcgen-tree2.dat");
            tree2.Write(fileName);
            tree1.Read(fileName);

            Assert.AreEqual(expSamplesCount, tree1.SamplesCount);

            ChanceTree ct1 = tree1.ConvertToChanceTree();
            ChanceTree ct2 = tree2.ConvertToChanceTree();

            // Compare two chance trees, they must be exactly the same.
            CompareChanceTrees cmp = new CompareChanceTrees();
            cmp.Compare(ct1, ct2);
            Assert.AreEqual(0, cmp.SumProbabDiff);
            for (int p = 0; p < chanceAbstractions.Length; ++p)
            {
                Assert.AreEqual(0, cmp.SumPotShareDiff[p]);
            }
        }

        [Test]
        public void Test_Leduc__FullGame()
        {
            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[]
            {
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),    
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame)
            };

            // Typical situation - many runs
            GenerateAndVerifyCT("fg", _leducHeGd, chanceAbstractions, true, 100000, 10, 0.01, 0.0, 0.005, true);
            // Do one run only
            GenerateAndVerifyCT("fg", _leducHeGd, chanceAbstractions, true, 1000000, 1, 0.01, 0.0, 0.005, true);
            // Extreme case - very short runs (skip read/write).
            GenerateAndVerifyCT("fg", _leducHeGd, chanceAbstractions, true, 10, 10000, 0.05, 0.0, 0.02, true);
        }

        [Test]
        public void Test_Leduc__FractionalResult()
        {
            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[]
            {
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult),    
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult)
            };
            // Typical situation - many runs
            GenerateAndVerifyCT("fr", _leducHeGd, chanceAbstractions, true, 200000, 10, 0.01, 0.001, 0.005, true);
            // Do one run only
            GenerateAndVerifyCT("fr", _leducHeGd, chanceAbstractions, true, 2000000, 1, 0.01, 0.001, 0.005, true);
            // Extreme case - very short runs (skip read/write).
            GenerateAndVerifyCT("fr", _leducHeGd, chanceAbstractions, true, 10, 10000, 0.05, 0.005, 0.02, true);
        }

        [Test]
        public void Test_Leduc__FullGame_FractionalResult()
        {
            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[]
            {
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),    
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FractionalResult)
            };
            // Typical situation - many runs
            GenerateAndVerifyCT("fgfr", _leducHeGd, chanceAbstractions, false, 100000, 10, 0.01, 0, 0.01, true);
            // Do one run only
            GenerateAndVerifyCT("fgfr", _leducHeGd, chanceAbstractions, false, 1000000, 1, 0.01, 0, 0.01, true);
            // Extreme case - very short runs (skip read/write).
            GenerateAndVerifyCT("fgfr", _leducHeGd, chanceAbstractions, false, 10, 10000, 0.05, 0, 0.05, true);
        }


        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Generate()
        {
            IChanceAbstraction[] chanceAbstractions = new IChanceAbstraction[]
            {
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame),    
                new LeducHeChanceAbstraction(LeducHeChanceAbstraction.FullGame)
            };

            int repCount = 5000000;
            DateTime startTime = DateTime.Now;
            CtMcGen.Generate(_leducHeGd, chanceAbstractions, false, repCount, 1, null);
            double time = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Repetitions: {0:0,0}, time: {1:0.0} s, {2:0,0} r/s", repCount, time, repCount / time);
        }

        #endregion

        #region Implementation

        GameDefinition _leducHeGd = XmlSerializerExt.Deserialize<GameDefinition>(
            Props.Global.Expand("${bds.DataDir}ai.pkr.metastrategy/${0}", "leduc-he.gamedef.xml"));

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "McCtGen_Test");

        /// <summary>
        /// This functions simulate a typical use case: create trees in many runs of MC sampling, write them, read again
        /// and merge into the master tree. The master tree is than verified.
        /// </summary>
        private void GenerateAndVerifyCT(string name, GameDefinition gd, IChanceAbstraction[] chanceAbstractions, bool areAbstractionsEqual, int samplesCount, int runsCount, double avRelProbabEps, double avPotShareEps, double eqValEps, bool visualize)
        {
            CtMcGen.Tree masterTree = new CtMcGen.Tree();

            int rngSeed = (int)DateTime.Now.Ticks;

            for (int run = 0; run < runsCount; ++run)
            {
                CtMcGen.Tree runTree = CtMcGen.Generate(gd, chanceAbstractions, areAbstractionsEqual, samplesCount, rngSeed, null);
                string fileName = Path.Combine(_outDir, String.Format("{0}-{1}-ct.dat", gd.Name, name));
                runTree.Write(fileName);
                masterTree.Read(fileName);

                // Do not use the timer anymore because the tests are too fast.
                rngSeed++; 
            }

            ChanceTree actCt = masterTree.ConvertToChanceTree();
            VisChanceTree.Show(actCt, Path.Combine(_outDir, String.Format("{0}-{1}-ct.gv", gd.Name, name)));
            VerifyChanceTree.VerifyS(actCt);
            ChanceTree expCt = CreateChanceTreeByAbstraction.CreateS(gd, chanceAbstractions);
            Assert.AreEqual(expCt.PlayersCount, actCt.PlayersCount);
            CompareChanceTrees cmp = new CompareChanceTrees();
            cmp.IsVerbose = visualize;
            cmp.Output = Console.Out;
            cmp.Compare(expCt, actCt);
            VisChanceTree.Show(expCt, Path.Combine(_outDir, String.Format("{0}-{1}-ct-exp.gv", gd.Name, name)));
            Assert.Less(cmp.AverageRelProbabDiff, avRelProbabEps);
            for (int p = 0; p < chanceAbstractions.Length; ++p)
            {
                Assert.Less(cmp.AveragePotShareDiff[p], avRelProbabEps);
            }

            ActionTree at = CreateActionTreeByGameDef.Create(gd);

            double [] actEqValues, expEqValues;
            EqLp.Solve(at, actCt, out actEqValues);
            EqLp.Solve(at, expCt, out expEqValues);
            for (int p = 0; p < chanceAbstractions.Length; ++p)
            {
                if (visualize)
                {
                    Console.WriteLine("Eq pos: {0} exp: {1} act: {2}", p, expEqValues[p], actEqValues[p]);
                }
                Assert.AreEqual(expEqValues[p], actEqValues[p], eqValEps);
            }
            if (visualize)
            {
                Console.WriteLine();
            }
        }

        #endregion
    }
}
