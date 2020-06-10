/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.holdem.strategy.core;
using ai.lib.utils;
using ai.pkr.metastrategy;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metastrategy.vis;
using System.IO;
using System.Reflection;

namespace ai.pkr.holdem.learn.nunit
{
    /// <summary>
    /// Unit tests for BigCardsPreflop. 
    /// </summary>
    [TestFixture]
    public class BigCardsPreflop_Test
    {
        #region Tests

        [Test]
        public void Test_CallKK()
        {
            string outDir = Path.Combine(_outDir, "call-KK");
            Directory.CreateDirectory(outDir);
            HePocketKind[] sbPockets = new HePocketKind[] { HePocketKind._AA, HePocketKind._KK, HePocketKind._AKs };
            HePocketKind[] bbPockets = new HePocketKind[] { HePocketKind._AA, HePocketKind._KK, HePocketKind._QQ, HePocketKind._AKs };

            string xmlAt = Props.Global.Expand("${bds.DataDir}ai.pkr.holdem.learn/bigcards-pf-1.xml");
            ActionTree at = XmlToActionTree.Convert(xmlAt);
            VisActionTree.Show(at, Path.Combine(outDir, "at.gv"));

            BigCardsPreflop bc = new BigCardsPreflop();
            bc.Solve(at, sbPockets, bbPockets);

            Console.WriteLine("Game values: {0}, {1}", bc.GameValues[0], bc.GameValues[1]);
            VisChanceTree.Show(bc.Ct, Path.Combine(outDir, "ct.gv"));
            VisStrategyTree.Show(bc.Strategies[0], Path.Combine(outDir, "st-0.gv"));
            VisStrategyTree.Show(bc.Strategies[1], Path.Combine(outDir, "st-1.gv"));
        }

        [Test]
        public void Test_AllIn()
        {
            string outDir = Path.Combine(_outDir, "all-in");
            Directory.CreateDirectory(outDir);

            HePocketKind[] sbPockets = new HePocketKind[] { HePocketKind._AA, HePocketKind._KK, HePocketKind._QQ, HePocketKind._AKs};
            HePocketKind[] bbPockets = new HePocketKind[] { HePocketKind._AA, HePocketKind._KK, HePocketKind._QQ, HePocketKind._JJ, 
                HePocketKind._AKs, HePocketKind._AKo };

            string xmlAt = Props.Global.Expand("${bds.DataDir}ai.pkr.holdem.learn/bigcards-pf-1.xml");
            ActionTree at = XmlToActionTree.Convert(xmlAt);
            VisActionTree.Show(at, Path.Combine(outDir, "at.gv"));

            BigCardsPreflop bc = new BigCardsPreflop();
            bc.Solve(at, sbPockets, bbPockets);

            Console.WriteLine("Game values: {0}, {1}", bc.GameValues[0], bc.GameValues[1]);
            VisChanceTree.Show(bc.Ct, Path.Combine(outDir, "ct.gv"));
            VisStrategyTree.Show(bc.Strategies[0], Path.Combine(outDir, "st-0.gv"));
            VisStrategyTree.Show(bc.Strategies[1], Path.Combine(outDir, "st-1.gv"));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "BigCardsPreflop_Test");

        #endregion
    }
}
