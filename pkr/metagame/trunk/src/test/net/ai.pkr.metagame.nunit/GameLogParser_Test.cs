/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using ai.lib.utils;

namespace ai.pkr.metagame.nunit
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public class GameLogParser_Test
    {
        #region Tests

        [Test]
        public void Test_ParseFile()
        {
            string subdirName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            string testResourcesPath = Path.Combine(Props.Global.Get("bds.TestDir"), subdirName);
            string logFile = Path.Combine(testResourcesPath, "gamelog1.log");
            GameLogParser parser = new GameLogParser();
            parser.OnMetaData += new GameLogParser.OnMetaDataHandler(parser_OnMetaData);
            parser.OnGameRecord += new GameLogParser.OnGameRecordHandler(parser_OnGameRecord);
            _eventCount = 0;
            parser.ParseFile(logFile);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        void parser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            switch (++_eventCount)
            {
                case 2:
                    Assert.AreEqual("0", gameRecord.Id);
                    break;
                case 4:
                    Assert.AreEqual("1", gameRecord.Id);
                    break;
            }
        }

        void parser_OnMetaData(GameLogParser source, string metaData)
        {
            switch (++_eventCount)
            {
                case 1:
                    Assert.AreEqual("Meta-data1", metaData);
                    break;
                case 3:
                    Assert.AreEqual("Meta-data2", metaData);
                    break;
            }
        }
        private int _eventCount;
        #endregion
    }
}
