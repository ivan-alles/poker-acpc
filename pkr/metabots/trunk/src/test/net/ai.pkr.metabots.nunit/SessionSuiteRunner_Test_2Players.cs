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
    /// Tests a game between 2 player performed by SessionSuiteRunner.
    /// </summary>
    [TestFixture]
    public class SessionSuiteRunner_Test_2Players
    {
        #region Tests
        [Test]
        public void Test_Game()
        {
            string testResourcesPath = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());

            instance = this;

            SessionSuiteRunner = new SessionSuiteRunner();
            SessionSuiteRunner.Configuration = XmlSerializerExt.Deserialize<SessionSuiteCfg>(
                Path.Combine(testResourcesPath, "SessionSuiteRunner_Test_2Players.Sessions.xml"));
            SessionSuiteRunner.Run();

            int totalCardsCount = CalculateTotalCardsCount();

            for (int p = 0; p < 2; ++p)
            {
                Assert.AreEqual(1, _players[p].OnGameBeginCount);
                Assert.AreEqual(1, _players[p].OnGameEndCount);
            }
            // Guest player did not play.
            Assert.AreEqual(0, _players[2].OnGameBeginCount);
            Assert.AreEqual(0, _players[2].OnGameEndCount);
        }
        #endregion

        private int CalculateTotalCardsCount()
        {
            int totalCardsCount = 0;
            for (int r = 0; r < _gameDefinition.RoundsCount; ++r)
            {
                totalCardsCount +=
                    2 * (_gameDefinition.PrivateCardsCount[r] + _gameDefinition.PublicCardsCount[r]) +
                    _gameDefinition.SharedCardsCount[r];
            }
            return totalCardsCount;
        }

        #region IGameRules implementation
        public class GameRules: GameRulesHelper
        {
            #region IGameRules Members

            public override void Showdown(GameDefinition gameDefinition, int[][] hands, UInt32[] ranks)
            {
                instance._gameDefinition = gameDefinition;
                base.Showdown(gameDefinition, hands, ranks);
                // Let Player at pos 0 win.
                ranks[0] = 2;
                ranks[1] = 1;
            }

            #endregion
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
                instance._gameDefinition = gameDef;
            }

            public override void OnGameBegin(string gameString)
            {
                base.OnGameBegin(gameString);
                if (Name == "Player0")
                {
                    Assert.AreEqual(-.5, CurGameState.Players[Position].Stack);
                    Assert.AreEqual(0.5, CurGameState.Players[Position].InPot);
                }
                else
                {
                    Assert.AreEqual(-1, CurGameState.Players[Position].Stack);
                    Assert.AreEqual(1, CurGameState.Players[Position].InPot);
                }
                Assert.AreEqual(0, CurGameState.Players[Position].Result);
                Assert.AreEqual("0", CurGameState.Id);
            }

            public override PokerAction OnActionRequired(string gameString)
            {
                // Program and verify the following moves.
                //
                // 0 Round: SB r, BB r, SB c
                // 1 Round: BB r, SB r, BB c

                base.OnActionRequired(gameString);
                if(Name == "Player0")
                {
                    // Player0 is SB
                    switch (OnActionRequiredCount)
                    {
                        // Round 0
                        case 1:
                            VerifyActions(gameString, 2, null, "? ?");

                            Assert.AreEqual(0, CurGameState.Round);
                            Assert.AreEqual(1.5, CurGameState.Pot);
                            Assert.AreEqual(1, CurGameState.Bet);
                            Assert.AreEqual(1, CurGameState.BetCount);
                            Assert.AreEqual(Position, CurGameState.CurrentActor);
                            Assert.AreEqual(1, CurGameState.GetMinBet(GameDefinition));
                            Assert.AreEqual(1, CurGameState.GetMaxBet(GameDefinition));

                            Assert.AreEqual(true, CurGameState.Players[0].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[0].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[0].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[0].IsAllIn);
                            Assert.AreEqual(0.5, CurGameState.Players[0].InPot);
                            Assert.AreEqual(0.5, CurGameState.Players[0].Bet);
                            Assert.AreEqual(-.5, CurGameState.Players[0].Stack);

                            Assert.AreEqual(true, CurGameState.Players[1].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[1].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[1].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[1].IsAllIn);
                            Assert.AreEqual(1, CurGameState.Players[1].InPot);
                            Assert.AreEqual(1, CurGameState.Players[1].Bet);
                            Assert.AreEqual(-1, CurGameState.Players[1].Stack);

                            return PokerAction.r(0, 1);
                        case 2:
                            VerifyActions(gameString, 4, null, "? ?");

                            Assert.AreEqual(0, CurGameState.Round);
                            Assert.AreEqual(5, CurGameState.Pot);
                            Assert.AreEqual(3, CurGameState.Bet);
                            Assert.AreEqual(3, CurGameState.BetCount);
                            Assert.AreEqual(Position, CurGameState.CurrentActor);
                            Assert.AreEqual(1, CurGameState.GetMinBet(GameDefinition));
                            Assert.AreEqual(1, CurGameState.GetMaxBet(GameDefinition));

                            Assert.AreEqual(true, CurGameState.Players[0].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[0].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[0].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[0].IsAllIn);
                            Assert.AreEqual(2, CurGameState.Players[0].InPot);
                            Assert.AreEqual(2, CurGameState.Players[0].Bet);
                            Assert.AreEqual(-2, CurGameState.Players[0].Stack);

                            Assert.AreEqual(false, CurGameState.Players[1].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[1].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[1].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[1].IsAllIn);
                            Assert.AreEqual(3, CurGameState.Players[1].InPot);
                            Assert.AreEqual(3, CurGameState.Players[1].Bet);
                            Assert.AreEqual(-3, CurGameState.Players[1].Stack);

                            return PokerAction.c(0);
                        // Round 1
                        case 3:
                            VerifyActions(gameString, 7, null, "? ?");

                            Assert.AreEqual(1, CurGameState.Round);
                            Assert.AreEqual(8, CurGameState.Pot);
                            Assert.AreEqual(2, CurGameState.Bet);
                            Assert.AreEqual(1, CurGameState.BetCount);
                            Assert.AreEqual(Position, CurGameState.CurrentActor);
                            Assert.AreEqual(2, CurGameState.GetMinBet(GameDefinition));
                            Assert.AreEqual(2, CurGameState.GetMaxBet(GameDefinition));

                            Assert.AreEqual(true, CurGameState.Players[0].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[0].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[0].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[0].IsAllIn);
                            Assert.AreEqual(3, CurGameState.Players[0].InPot);
                            Assert.AreEqual(0, CurGameState.Players[0].Bet);
                            Assert.AreEqual(-3, CurGameState.Players[0].Stack);
                            Assert.AreEqual(14, CurGameState.Players[0].Hand.Length);

                            Assert.AreEqual(false, CurGameState.Players[1].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[1].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[1].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[1].IsAllIn);
                            Assert.AreEqual(5, CurGameState.Players[1].InPot);
                            Assert.AreEqual(2, CurGameState.Players[1].Bet);
                            Assert.AreEqual(-5, CurGameState.Players[1].Stack);
                            Assert.AreEqual(12, CurGameState.Players[1].Hand.Length);
                            Assert.IsTrue(CurGameState.Players[1].Hand.StartsWith("? ?"));

                            return PokerAction.r(0, 1);
                    }
                }
                else
                {
                    // Player1 is BB
                    switch (OnActionRequiredCount)
                    {
                        // Round 0
                        case 1:
                            VerifyActions(gameString, 3, "? ?", null);

                            Assert.AreEqual(0, CurGameState.Round);
                            Assert.AreEqual(3, CurGameState.Pot);
                            Assert.AreEqual(2, CurGameState.Bet);
                            Assert.AreEqual(2, CurGameState.BetCount);
                            Assert.AreEqual(Position, CurGameState.CurrentActor);
                            Assert.AreEqual(1, CurGameState.GetMinBet(GameDefinition));
                            Assert.AreEqual(1, CurGameState.GetMaxBet(GameDefinition));

                            Assert.AreEqual(false, CurGameState.Players[0].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[0].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[0].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[0].IsAllIn);
                            Assert.AreEqual(2, CurGameState.Players[0].InPot);
                            Assert.AreEqual(2, CurGameState.Players[0].Bet);
                            Assert.AreEqual(-2, CurGameState.Players[0].Stack);
                            Assert.AreEqual("? ?", CurGameState.Players[0].Hand);

                            Assert.AreEqual(true, CurGameState.Players[1].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[1].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[1].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[1].IsAllIn);
                            Assert.AreEqual(1, CurGameState.Players[1].InPot);
                            Assert.AreEqual(1, CurGameState.Players[1].Bet);
                            Assert.AreEqual(-1, CurGameState.Players[1].Stack);
                            Assert.AreEqual(5, CurGameState.Players[1].Hand.Length);

                            return PokerAction.r(0, 1);
                        // Round 1
                        case 2:
                            VerifyActions(gameString, 6, "? ?", null);

                            Assert.AreEqual(1, CurGameState.Round);
                            Assert.AreEqual(6, CurGameState.Pot);
                            Assert.AreEqual(0, CurGameState.Bet);
                            Assert.AreEqual(0, CurGameState.BetCount);
                            Assert.AreEqual(Position, CurGameState.CurrentActor);
                            Assert.AreEqual(2, CurGameState.GetMinBet(GameDefinition));
                            Assert.AreEqual(2, CurGameState.GetMaxBet(GameDefinition));

                            Assert.AreEqual(true, CurGameState.Players[0].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[0].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[0].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[0].IsAllIn);
                            Assert.AreEqual(3, CurGameState.Players[0].InPot);
                            Assert.AreEqual(0, CurGameState.Players[0].Bet);
                            Assert.AreEqual(-3, CurGameState.Players[0].Stack);
                            Assert.AreEqual(14, CurGameState.Players[1].Hand.Length);

                            Assert.AreEqual(true, CurGameState.Players[1].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[1].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[1].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[1].IsAllIn);
                            Assert.AreEqual(3, CurGameState.Players[1].InPot);
                            Assert.AreEqual(0, CurGameState.Players[1].Bet);
                            Assert.AreEqual(-3, CurGameState.Players[1].Stack);
                            Assert.AreEqual(14, CurGameState.Players[1].Hand.Length);

                            return PokerAction.r(0, 1);
                        case 3:
                            VerifyActions(gameString, 8, "? ?", null);

                            Assert.AreEqual(1, CurGameState.Round);
                            Assert.AreEqual(12, CurGameState.Pot);
                            Assert.AreEqual(4, CurGameState.Bet);
                            Assert.AreEqual(2, CurGameState.BetCount);
                            Assert.AreEqual(Position, CurGameState.CurrentActor);
                            Assert.AreEqual(2, CurGameState.GetMinBet(GameDefinition));
                            Assert.AreEqual(2, CurGameState.GetMaxBet(GameDefinition));

                            Assert.AreEqual(false, CurGameState.Players[0].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[0].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[0].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[0].IsAllIn);
                            Assert.AreEqual(7, CurGameState.Players[0].InPot);
                            Assert.AreEqual(4, CurGameState.Players[0].Bet);
                            Assert.AreEqual(-7, CurGameState.Players[0].Stack);
                            Assert.AreEqual(12, CurGameState.Players[0].Hand.Length);
                            Assert.IsTrue(CurGameState.Players[0].Hand.StartsWith("? ?"));

                            Assert.AreEqual(true, CurGameState.Players[1].CanActInCurrentRound);
                            Assert.AreEqual(true, CurGameState.Players[1].CanActInGame);
                            Assert.AreEqual(false, CurGameState.Players[1].IsFolded);
                            Assert.AreEqual(false, CurGameState.Players[1].IsAllIn);
                            Assert.AreEqual(5, CurGameState.Players[1].InPot);
                            Assert.AreEqual(2, CurGameState.Players[1].Bet);
                            Assert.AreEqual(-5, CurGameState.Players[1].Stack);
                            Assert.AreEqual(14, CurGameState.Players[1].Hand.Length);

                            return PokerAction.c(0);
                    }
                }
                Assert.Fail();
                return null;
            }

            public override void OnGameEnd(string gameString)
            {
                base.OnGameEnd(gameString);
                Assert.AreEqual(_privateCards, CurGameState.Players[Position].Hand.Substring(0, 5));
                VerifyActions(gameString, 9, null, null);

                Assert.AreEqual(1, CurGameState.Round);
                Assert.AreEqual(14, CurGameState.Pot);
                Assert.AreEqual(4, CurGameState.Bet);
                Assert.AreEqual(2, CurGameState.BetCount);
                Assert.AreEqual(2, CurGameState.GetMinBet(GameDefinition));
                Assert.AreEqual(2, CurGameState.GetMaxBet(GameDefinition));

                Assert.AreEqual(false, CurGameState.Players[0].CanActInCurrentRound);
                Assert.AreEqual(true, CurGameState.Players[0].CanActInGame);
                Assert.AreEqual(false, CurGameState.Players[0].IsFolded);
                Assert.AreEqual(false, CurGameState.Players[0].IsAllIn);
                Assert.AreEqual(7, CurGameState.Players[0].InPot);
                Assert.AreEqual(4, CurGameState.Players[0].Bet);
                Assert.AreEqual(7, CurGameState.Players[0].Stack);
                Assert.AreEqual(7, CurGameState.Players[0].Result);
                Assert.AreEqual(14, CurGameState.Players[0].Hand.Length);

                Assert.AreEqual(false, CurGameState.Players[1].CanActInCurrentRound);
                Assert.AreEqual(true, CurGameState.Players[1].CanActInGame);
                Assert.AreEqual(false, CurGameState.Players[1].IsFolded);
                Assert.AreEqual(false, CurGameState.Players[1].IsAllIn);
                Assert.AreEqual(7, CurGameState.Players[1].InPot);
                Assert.AreEqual(4, CurGameState.Players[1].Bet);
                Assert.AreEqual(-7, CurGameState.Players[1].Stack);
                Assert.AreEqual(-7, CurGameState.Players[1].Result);
                Assert.AreEqual(14, CurGameState.Players[1].Hand.Length);
            }

            void VerifyActions(string gameString, int expectedCount, string expectedDeal0, string expectedDeal1)
            {
                GameRecord gameRecord = GameRecord.Parse(gameString);
                Assert.AreEqual(expectedCount, gameRecord.Actions.Count);
                for (int a = 0; a < gameRecord.Actions.Count; ++a)
                {
                    PokerAction ar = gameRecord.Actions[a];
                    if(ar.Kind == Ak.d && Position == ar.Position)
                    {
                        _privateCards = ar.Cards;
                    }
                    switch(a)
                    {
                        // Round 0
                        case 0:
                            Assert.AreEqual(0, ar.Position);
                            Assert.AreEqual(Ak.d, ar.Kind);
                            Assert.AreEqual(0, ar.Amount);
                            if(expectedDeal0 == null)
                                Assert.AreEqual(5, ar.Cards.Length);
                            else
                                Assert.AreEqual(expectedDeal0, ar.Cards);
                            break;
                        case 1:
                            Assert.AreEqual(1, ar.Position);
                            Assert.AreEqual(Ak.d, ar.Kind);
                            Assert.AreEqual(0, ar.Amount);
                            if (expectedDeal1 == null)
                                Assert.AreEqual(5, ar.Cards.Length);
                            else
                                Assert.AreEqual(expectedDeal1, ar.Cards);
                            break;
                        case 2:
                            Assert.AreEqual(0, ar.Position);
                            Assert.AreEqual(Ak.r, ar.Kind);
                            Assert.AreEqual(1, ar.Amount);
                            break;
                        case 3:
                            Assert.AreEqual(1, ar.Position);
                            Assert.AreEqual(Ak.r, ar.Kind);
                            Assert.AreEqual(1, ar.Amount);
                            break;
                        case 4:
                            Assert.AreEqual(0, ar.Position);
                            Assert.AreEqual(Ak.c, ar.Kind);
                            Assert.AreEqual(0, ar.Amount);
                            break;
                        // Round 1
                        case 5:
                            Assert.AreEqual(-1, ar.Position);
                            Assert.AreEqual(Ak.d, ar.Kind);
                            Assert.AreEqual(0, ar.Amount);
                            Assert.AreEqual(8, ar.Cards.Length);
                            break;
                        case 6:
                            Assert.AreEqual(1, ar.Position);
                            Assert.AreEqual(Ak.r, ar.Kind);
                            Assert.AreEqual(2, ar.Amount);
                            break;
                        case 7:
                            Assert.AreEqual(0, ar.Position);
                            Assert.AreEqual(Ak.r, ar.Kind);
                            Assert.AreEqual(2, ar.Amount);
                            break;
                        case 8:
                            Assert.AreEqual(1, ar.Position);
                            Assert.AreEqual(Ak.c, ar.Kind);
                            Assert.AreEqual(0, ar.Amount);
                            break;
                    }
                }

            }
            internal string _privateCards;
        }
        #endregion

        static internal SessionSuiteRunner_Test_2Players instance;
        internal SessionSuiteRunner SessionSuiteRunner;
        internal List<Player> _players = new List<Player>();
        internal GameDefinition _gameDefinition;
    }
}
