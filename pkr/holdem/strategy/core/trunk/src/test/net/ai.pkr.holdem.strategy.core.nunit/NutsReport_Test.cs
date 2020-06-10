/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using System.Reflection;
using ai.pkr.metagame;
using System.IO;

namespace ai.pkr.holdem.strategy.core.nunit
{
    /// <summary>
    /// Unit tests for NutsReport.
    /// </summary>
    [TestFixture]
    public class NutsReport_Test
    {
        #region Tests

        /// <summary>
        /// Runs a report without verification.
        /// </summary>
        [Test]
        public void Test_Preview()
        {
            GameLogParser parser = new GameLogParser();
            parser.OnGameRecord += new GameLogParser.OnGameRecordHandler(parser_OnGameRecord);

            Props p = new Props(new string[] { "HeroName", "Patience", "PrintNuts", "true" });
            _report = new NutsReport();
            _report.Configure(p);

            parser.ParseFile(Path.Combine(_testResDir, "NutsReport_Log1.log"));
            Assert.AreEqual(0, parser.ErrorCount);

            _report.Print(Console.Out);
        }

        void parser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            _report.Update(gameRecord);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
        NutsReport _report;

        #endregion
    }
}
