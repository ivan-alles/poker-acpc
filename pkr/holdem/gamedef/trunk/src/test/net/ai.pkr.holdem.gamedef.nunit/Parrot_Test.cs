/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using ai.pkr.metabots;
using ai.lib.utils;
using ai.pkr.metatools;
using ICSharpCode.SharpZipLib.Zip;

namespace ai.pkr.holdem.gamedef.nunit
{
    /// <summary>
    /// This test replays games recorded elsewhere and verifies that the game play is the same.
    /// </summary>
    [TestFixture]
    public class Parrot_Test
    {
        #region Tests

        [Test]
        public void Test_Replay()
        {
            Replay("pa.mv.he.fe.2");
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        // Naming convention:
        // Log file       : baseName.log
        // Zipped log file: baseName.zip
        // Session suite  : baseName-ss.xml
        void Replay(string baseName)
        {
            string testResourcesPath = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());
            string tempDir = Path.GetTempPath();

            string configZip = Path.Combine(testResourcesPath, baseName + ".zip");

            FastZip fz = new FastZip();
            fz.ExtractZip(configZip, tempDir, FastZip.Overwrite.Always, null, "", "", true);
            string origLogPath = Path.Combine(tempDir, baseName + ".log");

            string ssConfigFile = Path.Combine(testResourcesPath, baseName + "-ss.xml");
            _ssRunner = new SessionSuiteRunner();
            _ssRunner.Configuration = XmlSerializerExt.Deserialize<SessionSuiteCfg>(ssConfigFile);
            _ssRunner.IsLoggingEnabled = true;
            _ssRunner.LogDir = tempDir;

            // Replace XML path to the log being replayed with the actual path from the temp directory.
            _ssRunner.Configuration.Sessions[0].ReplayFrom = origLogPath;
            for (int p = 0; p < _ssRunner.Configuration.LocalPlayers.Length; ++p)
            {
                _ssRunner.Configuration.LocalPlayers[p].CreationParameters.Set("ReplayFrom", origLogPath);
            }

            _ssRunner.Run();

            string hint;
            bool comparisonResult = GameLogComparer.Compare(_ssRunner.Configuration.Sessions[0].ReplayFrom.Get(Props.Global),
                                                            _ssRunner.CurrentLogFile, out hint, int.MaxValue);
            Console.WriteLine("Original log:" + _ssRunner.Configuration.Sessions[0].ReplayFrom);
            Console.WriteLine("Replayed log:" + _ssRunner.CurrentLogFile);
            Console.WriteLine(hint);
            Assert.IsTrue(comparisonResult);
        }

        private SessionSuiteRunner _ssRunner;
        #endregion
    }
}

