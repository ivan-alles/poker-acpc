/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml.Serialization;
using System.IO;
using ai.lib.utils;
using System.Reflection;

namespace ai.pkr.metabots.nunit
{
    /// <summary>
    /// Unit tests for SessionSuiteCfg. 
    /// </summary>
    [TestFixture]
    public class SessionSuiteCfg_Test
    {
        #region Tests
        
        [Test]
        public void Test_XmlSerialization()
        {
            SessionSuiteCfg ssc1 = new SessionSuiteCfg();

            string testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());

            ssc1.Name = "Suite1";
            ssc1.LocalPlayers = new LocalPlayerCfg[2]
                                    {
                                        new LocalPlayerCfg
                                            {
                                                Assembly = "Players.dll",
                                                Type = "SuperBot",
                                                Name = "SuperBot1",
                                                CreationParameters = new string[]{"Param1", "1"}
                                            },
                                        new LocalPlayerCfg
                                            {
                                                Assembly = "Players.dll",
                                                Type = "PuperBot",
                                                Name = "PuperBot1",
                                                CreationParameters = new Dictionary<string, string>{{"Param1", "2"}}
                                            }
            };

            ssc1.Sessions = new SessionCfg[2]
                               {
                                   new SessionCfg("Ring Game", SessionKind.RingGame, testResDir + "SessionSuiteCfg_Test.Gamedef1.xml"),
                                   new SessionCfg("Ring Game With Seat Permutation", 
                                       SessionKind.RingGameWithSeatPermutations, testResDir + "SessionSuiteCfg_Test.Gamedef1.xml")
                               };

            ssc1.Sessions[0].GamesCount = 1000;
            ssc1.Sessions[0].Players = new PlayerSessionCfg[]
                                       {
                                           new PlayerSessionCfg{ Name = "SuperBot1", SessionParameters = new Dictionary<string, string>{{"p1", "v1"}}},
                                           new PlayerSessionCfg{ Name = "PuperBot1", SessionParameters = new Dictionary<string, string>{{"p1", "v1"}}}
                                       };

            ssc1.Sessions[1].GamesCount = 100;
            ssc1.Sessions[1].Players = new PlayerSessionCfg[]
                                       {
                                           new PlayerSessionCfg{ Name = "SuperBot1", SessionParameters = new Dictionary<string, string>{{"p1", "v1"}}},
                                           new PlayerSessionCfg{ Name = "PuperBot1", SessionParameters = new Dictionary<string, string>{{"p1", "v1"}}}
                                       };



            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                ssc1.XmlSerialize(tw);
            }

            Console.WriteLine(sb.ToString());
            
            SessionSuiteCfg rc2;

            using (TextReader textReader = new StringReader(sb.ToString()))
            {
                XmlSerializerExt.Deserialize(out rc2, textReader);
            }

            Assert.IsNotNull(rc2);
        }

        [Test]
        public void Test_DeserializeHandWrittenWithSchema()
        {
            string testResourcesPath = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());

            SessionSuiteCfg ssc = XmlSerializerExt.Deserialize<SessionSuiteCfg>(
                Path.Combine(testResourcesPath, "SessionSuiteCfg_Test.xml"));
            
            Assert.IsNotNull(ssc);
            Assert.AreEqual("Test Suite", ssc.Name);
            Assert.AreEqual(2, ssc.LocalPlayers.Length);
            Assert.AreEqual("RaiserBot1", ssc.LocalPlayers[0].Name);
            Assert.AreEqual("players.dll", ssc.LocalPlayers[0].Assembly.RawValue);
            Assert.AreEqual("players.RaiserBot", ssc.LocalPlayers[0].Type.RawValue);
            Assert.AreEqual(1, ssc.LocalPlayers[0].CreationParameters.Count);
            Assert.AreEqual("Some creation configuration", ssc.LocalPlayers[0].CreationParameters.Get("p1"));
            Assert.AreEqual(2, ssc.Sessions.Length);
            // Session 0
            Assert.AreEqual("Ring Game", ssc.Sessions[0].Name);
            Assert.AreEqual(SessionKind.RingGame, ssc.Sessions[0].Kind);
            Assert.AreEqual(1000, ssc.Sessions[0].GamesCount);
            Assert.AreEqual(36, ssc.Sessions[0].RngSeed);
            Assert.AreEqual(2, ssc.Sessions[0].Players.Length);
            Assert.AreEqual("RaiserBot1", ssc.Sessions[0].Players[0].Name);
            Assert.AreEqual(2, ssc.Sessions[0].Players[0].SessionParameters.Count);
            Assert.AreEqual("v1", ssc.Sessions[0].Players[0].SessionParameters.Get("p1"));
            Assert.AreEqual("v2", ssc.Sessions[0].Players[0].SessionParameters.Get("p2"));
            Assert.AreEqual("GameDef1", ssc.Sessions[0].GameDefinition.Name);

            // Session 1
            Assert.AreEqual(0, ssc.Sessions[1].RngSeed);
            Assert.AreEqual("GameDef2", ssc.Sessions[1].GameDefinition.Name);
        }

        #endregion
    }
}
