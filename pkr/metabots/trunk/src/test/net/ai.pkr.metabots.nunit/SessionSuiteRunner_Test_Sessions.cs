/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using System.IO;
using NUnit.Framework;
using ai.lib.utils;
using System.Reflection;

namespace ai.pkr.metabots.nunit
{
    /// <summary>
    /// Test for class SessionSuiteRunner. 
    /// Tests creation of players and sessions.
    /// </summary>
    [TestFixture]
    public class SessionSuiteRunner_Test_Sessions
    {
        #region Tests
        [Test]
        public void Test_Session()
        {
            string testResourcesPath = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());

            instance = this;
            SessionSuiteRunner = new SessionSuiteRunner();
            SessionSuiteRunner.Configuration = XmlSerializerExt.Deserialize<SessionSuiteCfg>(
                Path.Combine(testResourcesPath, "SessionSuiteRunner_Test_Sessions.xml"));

            SessionSuiteRunner.Run();

            Assert.AreEqual(2, _players.Count);


            Assert.IsNotNull(_gameRules1);
            Assert.AreEqual(1, _gameRules1.OnCreateCount);

            Assert.IsNotNull(_gameRules2);
            Assert.AreEqual(1, _gameRules2.OnCreateCount);

            for (int p = 0; p < _players.Count; ++p)
            {
                Assert.AreEqual(1, _players[p].OnServerConnectCount);
                Assert.AreEqual(1, _players[p].OnServerDisconnectCount);
                Assert.AreEqual(2, _players[p].OnSessionBeginCount);
                Assert.AreEqual(2, _players[p].OnSessionEndCount);
                Assert.AreEqual(5, _players[p].OnGameBeginCount);
                Assert.AreEqual(10, _players[p].OnActionRequiredCount);
                Assert.AreEqual(5, _players[p].OnGameEndCount);
            }
        }
        #endregion
        #region IGameRules implementation
        public class GameRules1: GameRulesHelper
        {
            #region IGameRules Members

            public override void OnCreate(Props configString)
            {
                base.OnCreate(configString);
                Assert.AreEqual(1, configString.Count);
                Assert.AreEqual("CreationParams1", configString.Get("p1"));
                instance._gameRules1 = this;
            }

            public override void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
            {
                base.Showdown(gameDefinition, hands, ranks);
                // Let Player0 win.
                int winner = _player0Pos;
                _player0Pos = (_player0Pos + 1) % 2;
                ranks[winner] = 2;
                ranks[1 - winner] = 1;
            }

            #endregion

            int _player0Pos = 0;
        }

        public class GameRules2 : GameRulesHelper
        {
            #region IGameRules Members

            public override void OnCreate(Props configString)
            {
                base.OnCreate(configString);
                Assert.AreEqual("CreationParams2", configString.Get("p1"));
                instance._gameRules2 = this;
            }

            public override void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
            {
                base.Showdown(gameDefinition, hands, ranks);
                // Let Player1 win.
                int winner = 1 - _player0Pos;
                _player0Pos = (_player0Pos + 1) % 2;
                ranks[winner] = 2;
                ranks[1 - winner] = 1;
            }
            #endregion

            int _player0Pos = 0;
        }

        #endregion

        #region IPlayer implementation
        public class Player: PlayerHelper
        {
            public override void OnCreate(string name, Props creationParameters)
            {
                base.OnCreate(name, creationParameters);
                instance._players.Add(this);
            }

            public override void OnSessionBegin(string sessionName, GameDefinition gameDef, Props sessionParameters)
            {
                base.OnSessionBegin(sessionName, gameDef, sessionParameters);
                switch(OnSessionBeginCount)
                {
                    case 1:
                        Assert.AreEqual("Session1", sessionName);
                        Assert.AreEqual("RoomTest.GameDef1", gameDef.Name);
                        Assert.AreEqual(0, OnGameBeginCount);
                        Assert.AreEqual(0, OnGameEndCount);
                        break;
                    case 2:
                        Assert.AreEqual("Session2", sessionName);
                        Assert.AreEqual("RoomTest.GameDef2", gameDef.Name);
                        Assert.AreEqual(2, OnGameBeginCount);
                        Assert.AreEqual(2, OnGameEndCount);
                        break;
                }
            }

            public override void OnGameBegin(string gameString)
            {
                base.OnGameBegin(gameString);
                Assert.AreEqual(Name, CurGameState.Players[Position].Name);
                switch(OnGameBeginCount)
                {
                    // Session 1 - Player0  wins
                    case 1:
                        if (Name == "Player0")
                            Assert.AreEqual(-0.5, CurGameState.Players[Position].Stack);
                        else
                            Assert.AreEqual(-1, CurGameState.Players[Position].Stack);
                        break;
                    case 2:
                        if (Name == "Player0")
                            Assert.AreEqual(0, CurGameState.Players[Position].Stack);
                        else
                            Assert.AreEqual(-1.5, CurGameState.Players[Position].Stack);
                        break;
                    // Session 2 - Player1 wins
                    case 3:
                        if (Name == "Player0")
                            Assert.AreEqual(-.5, CurGameState.Players[Position].Stack);
                        else
                            Assert.AreEqual(-1, CurGameState.Players[Position].Stack);
                        break;
                    case 4:
                        if (Name == "Player0")
                            Assert.AreEqual(-2, CurGameState.Players[Position].Stack);
                        else
                            Assert.AreEqual(0.5, CurGameState.Players[Position].Stack);
                        break;
                    case 5:
                        if (Name == "Player0")
                            Assert.AreEqual(-2.5, CurGameState.Players[Position].Stack);
                        else
                            Assert.AreEqual(1, CurGameState.Players[Position].Stack);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }

            public override PokerAction OnActionRequired(string gameString)
            {
                Assert.AreEqual(Name, CurGameState.Players[Position].Name);
                base.OnActionRequired(gameString);
                return PokerAction.c(0);
            }

        }
        #endregion

        static internal SessionSuiteRunner_Test_Sessions instance;
        internal SessionSuiteRunner SessionSuiteRunner;
        internal List<Player> _players = new List<Player>();
        internal GameRules1 _gameRules1;
        internal GameRules2 _gameRules2;
    }
}
