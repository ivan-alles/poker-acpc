/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ai.lib.utils;
using System.Reflection;
using System.Diagnostics;

namespace ai.pkr.metagame.nunit
{
    /// <summary>
    /// Unit test for GameState.
    /// As PlayerState is a part of GameState, it is tested here as well.
    /// </summary>
    [TestFixture]
    public class GameState_Test
    {
        #region Tests

        [Test]
        public void Test_Equals()
        {
            string gameString = "0;P1{0 5 0} P2{0 10 0};0d{Ah Ad} 1d{Kh Kd} 0r13 1r10;";
            
            GameState gs1 = new GameState(gameString, null);
            Assert.IsTrue(gs1.Equals(gs1));
            Assert.IsFalse(gs1.Equals(null));
            Assert.IsFalse(gs1.Equals(4));
            
            GameState gs2 = new GameState(gameString, null);
            Assert.IsTrue(gs1.Equals(gs2));
            Assert.IsTrue(gs2.Equals(gs1));

            GameState gs3 = new GameState(gs1);
            Assert.IsTrue(gs1.Equals(gs3));
            Assert.IsTrue(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Id = "aaa";
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.IsGameOver = !gs3.IsGameOver;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Round++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Pot++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Bet++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.BetCount++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Pot ++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.LastActor++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.CurrentActor++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.IsDealerActing = !gs3.IsDealerActing;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.DealsCount++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.FoldedCount++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players = new PlayerState[] { new PlayerState() };
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].Name += "dsd";
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].Stack++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].Hand += "As";
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].Hand += "Kc";
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].Bet++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].InPot++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].Result++;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].IsFolded = !gs3.Players[0].IsFolded;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].IsAllIn = !gs3.Players[0].IsAllIn;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));

            gs3 = new GameState(gs1);
            gs3.Players[0].CanActInCurrentRound = !gs3.Players[0].CanActInCurrentRound;
            Assert.IsFalse(gs1.Equals(gs3));
            Assert.IsFalse(gs3.Equals(gs1));
        }

        [Test]
        public void Test_CopyCostructor()
        {
            string gameString =
                "#133;FellOmen2{20 5 200} Probe{-20 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10 1c d{5c 8d 6s} 1c 0r10 1r10 0c d{Th} 1c 0r20 1r20 0c d{2s} 1c 0c.";
            GameState gs1 = new GameState(gameString, null);
            GameState gs2 = new GameState(gs1);
            GameState gs3 = gs1.DeepCopy();
            Assert.AreEqual(gs3, gs2);
        }

        [Test]
        public void Test_XmlSerialization()
        {
            GameState gs1 = new GameState("0;P1{100 5 0} P2{200 10 0}; 0d{Ah Ad} 1d{Kh Kd} 0r13 1r10 0c d{2c 3d 5h};", null);

            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                gs1.XmlSerialize(tw);
            }

            Console.WriteLine(sb.ToString());

            GameState gs2;

            using (TextReader tr = new StringReader(sb.ToString()))
            {
                XmlSerializerExt.Deserialize(out gs2, tr);
            }

            Assert.AreEqual(gs1, gs2);
        }

        [Test]
        public void Test_GameString_BlindBet()
        {
            // Non-equal blinds
            string gameString = "11;P1{100 5 0} P2{200 10 0};;";
            GameState gs = new GameState(gameString, null);
            Assert.AreEqual(10, gs.Bet);
            Assert.AreEqual(1, gs.BetCount);

            // Equal blinds
            gameString = "11;P1{100 5 0} P2{200 5 0};;";
            gs = new GameState(gameString, null);
            Assert.AreEqual(5, gs.Bet);
            Assert.AreEqual(1, gs.BetCount);

            // Zero blinds
            gameString = "11;P1{100 0 0} P2{200 0 0};;";
            gs = new GameState(gameString, null);
            Assert.AreEqual(0, gs.Bet);
            Assert.AreEqual(0, gs.BetCount);
        }

        [Test]
        public void Test_GameString_NoGameDef_NotOver()
        {
            // As we do not have GameDef, CurrentActor == invalid and IsDealerAction is false.

            string gameString = "11;P1{100 5 0} P2{200 10 0}; 0d{Ah Ad} 1d{Kh Kd} 0r13 1r10;";
            
            GameState gs = new GameState(gameString, null);

            VerifyGameState(gs, "11", false, 0, 56, 33, 3, 1, false, GameState.InvalidPosition, 0, 0, false, 2);
            VerifyPlayerState(gs.Players[0], "P1", 77, "Ah Ad", 23, 23, 0, false, false, true, true);
            VerifyPlayerState(gs.Players[1], "P2", 167, "Kh Kd", 33, 33, 0, false, false, false, true);

            gs.UpdateByAction(PokerAction.c(0), null);
            VerifyGameState(gs, "11", false, 0, 66, 33, 3, 0, true, GameState.InvalidPosition, 0, 0, false, 2);
            VerifyPlayerState(gs.Players[0], "P1", 67, "Ah Ad", 33, 33, 0, false, false, false, true);
            VerifyPlayerState(gs.Players[1], "P2", 167, "Kh Kd", 33, 33, 0, false, false, false, true);


            gs.UpdateByAction(PokerAction.d("As 2d 4d"), null);

            VerifyGameState(gs, "11", false, 1, 66, 0, 0, 
                GameState.DealerPosition, false, GameState.InvalidPosition, 1, 0, false, 2);
            VerifyPlayerState(gs.Players[0], "P1", 67, "Ah Ad As 2d 4d", 33, 0, 0, false, false, true, true);
            VerifyPlayerState(gs.Players[1], "P2", 167, "Kh Kd As 2d 4d", 33, 0, 0, false, false, true, true);


            gs.UpdateByAction(PokerAction.r(1, 15), null);
            VerifyGameState(gs, "11", false, 1, 81, 15, 1,
                1, false, GameState.InvalidPosition, 0, 0, false, 2);
            VerifyPlayerState(gs.Players[0], "P1", 67, "Ah Ad As 2d 4d", 33, 0, 0, false, false, true, true);
            VerifyPlayerState(gs.Players[1], "P2", 152, "Kh Kd As 2d 4d", 48, 15, 0, false, false, false, true);

            gs.UpdateByAction(PokerAction.f(0), null);
            VerifyGameState(gs, "11", true, 1, 81, 15, 1, 
                0, false, GameState.InvalidPosition, 0, 1, false, 2);
            VerifyPlayerState(gs.Players[0], "P1", 67, "Ah Ad As 2d 4d", 33, 0, -33, true, false, false, false);
            VerifyPlayerState(gs.Players[1], "P2", 233, "Kh Kd As 2d 4d", 48, 15, 33, false, false, false, true);
        }

        [Test]
        public void Test_GameString_GameDef_NotOver()
        {
            GameDefinition gameDef = LoadGameDef();

            string gameString = "14; P1{100 .5 0} P2{200 1 0}; 0d{Ac Ad} 1d{Kc Kd};";

            GameState gs = new GameState(gameString, gameDef);

            VerifyGameState(gs, "14", false, 0, 1.5, 1, 1, 1, false, 0, 2, 0, false, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] {Ak.f, Ak.c, Ak.r});
            VerifyPlayerState(gs.Players[0], "P1", 99.5, "Ac Ad", 0.5, 0.5, 0, false, false, true, true);
            VerifyPlayerState(gs.Players[1], "P2", 199, "Kc Kd", 1, 1, 0, false, false, true, true);

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            VerifyGameState(gs, "14", false, 0, 2, 1, 1, 0, false, 1, 0, 0, false, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] { Ak.c, Ak.r });
            VerifyPlayerState(gs.Players[0], "P1", 99, "Ac Ad", 1, 1, 0, false, false, false, true);
            VerifyPlayerState(gs.Players[1], "P2", 199, "Kc Kd", 1, 1, 0, false, false, true, true);

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            VerifyGameState(gs, "14", false, 0, 2, 1, 1, 1,  true, GameState.DealerPosition, 0, 0, false, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] { Ak.d });
            VerifyPlayerState(gs.Players[0], "P1", 99, "Ac Ad", 1, 1, 0, false, false, false, true);
            VerifyPlayerState(gs.Players[1], "P2", 199, "Kc Kd", 1, 1, 0, false, false, false, true);

            gs.UpdateByAction(PokerAction.d("As 2d 4d"), gameDef);
            VerifyGameState(gs, "14", false, 1, 2, 0, 0, GameState.DealerPosition, false, 1, 1, 0, false, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] { Ak.c, Ak.r });
            VerifyPlayerState(gs.Players[0], "P1", 99, "Ac Ad As 2d 4d", 1, 0, 0, false, false, true, true);
            VerifyPlayerState(gs.Players[1], "P2", 199, "Kc Kd As 2d 4d", 1, 0, 0, false, false, true, true);

            gs.UpdateByAction(PokerAction.r(1, gameDef.BetStructure[1]), gameDef);
            VerifyGameState(gs, "14", false, 1, 3, 1, 1, 1, false, 0, 0, 0, false, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] { Ak.f, Ak.c, Ak.r });
            VerifyPlayerState(gs.Players[0], "P1", 99, "Ac Ad As 2d 4d", 1, 0, 0, false, false, true, true);
            VerifyPlayerState(gs.Players[1], "P2", 198, "Kc Kd As 2d 4d", 2, 1, 0, false, false, false, true);

            gs.UpdateByAction(PokerAction.f(0), gameDef);
            VerifyGameState(gs, "14", true, 1, 3, 1, 1, 0, false, GameState.InvalidPosition, 0, 1, false, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] {});
            VerifyPlayerState(gs.Players[0], "P1", 99, "Ac Ad As 2d 4d", 1, 0, -1, true, false, false, false);
            VerifyPlayerState(gs.Players[1], "P2", 201, "Kc Kd As 2d 4d", 2, 1, 1, false, false, false, true);
        }

        [Test]
        public void Test_GameString_GameDef_Over()
        {
            GameDefinition gameDef = LoadGameDef();

            string gameString = "14; P1{100 .5 1} P2{200 1 -1}; 0d{Ac Ad} 1d{Kc Kd} 0c 1c d{Ah As 2c} 1c 0c d{3c} 1c 0c d{4d} 1c 0c.";

            GameState gs = new GameState(gameString, gameDef);

            VerifyGameState(gs, "14", true, 3, 2, 0, 0, 0, false, GameState.InvalidPosition, 0, 0, true, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] {});
            VerifyPlayerState(gs.Players[0], "P1", 101, "Ac Ad Ah As 2c 3c 4d", 1, 0, 1, false, false, false, true);
            VerifyPlayerState(gs.Players[1], "P2", 199, "Kc Kd Ah As 2c 3c 4d", 1, 0, -1, false, false, false, true);
        }

        [Test]
        public void Test_GameString_FromGameDef_Over()
        {
            GameDefinition gameDef = LoadGameDef();

            GameState gs = new GameState(gameDef, 2);

            VerifyGameState(gs, "", false, -1, 1.5, 1, 1, GameState.InvalidPosition, true, 0, 0, 0, false, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] {Ak.d });
            VerifyPlayerState(gs.Players[0], "", -0.5, "", 0.5, 0.5, 0, false, false, false, true);
            VerifyPlayerState(gs.Players[1], "", -1, "", 1, 1, 0, false, false, false, true);

            gs.UpdateByAction(PokerAction.d(0, "Ac Ad"), gameDef);
            gs.UpdateByAction(PokerAction.d(1, "2c 3d"), gameDef);

            VerifyPlayerState(gs.Players[0], "", -0.5, "Ac Ad", 0.5, 0.5, 0, false, false, true, true);
            VerifyPlayerState(gs.Players[1], "", -1, "2c 3d", 1, 1, 0, false, false, true, true);

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            gs.UpdateByAction(PokerAction.c(1), gameDef);
            gs.UpdateByAction(PokerAction.d("Ah As Qc"), gameDef);
            gs.UpdateByAction(PokerAction.c(1), gameDef);
            gs.UpdateByAction(PokerAction.c(0), gameDef);
            gs.UpdateByAction(PokerAction.d("Jh"), gameDef);
            gs.UpdateByAction(PokerAction.c(1), gameDef);
            gs.UpdateByAction(PokerAction.c(0), gameDef);
            gs.UpdateByAction(PokerAction.d("Ts"), gameDef);
            gs.UpdateByAction(PokerAction.c(1), gameDef);
            gs.UpdateByAction(PokerAction.c(0), gameDef);
            UInt32[] ranks = new UInt32[] { 2, 1 };
            gs.UpdateByShowdown(ranks, gameDef);
            VerifyGameState(gs, "", true, 3, 2, 0, 0, 0, false, GameState.InvalidPosition, 0, 0, true, 2);
            VerifyGameStateMethods(gs, gameDef, new Ak[] { });
            VerifyPlayerState(gs.Players[0], "", 1, "Ac Ad Ah As Qc Jh Ts", 1, 0, 1, false, false, false, true);
            VerifyPlayerState(gs.Players[1], "", -1, "2c 3d Ah As Qc Jh Ts", 1, 0, -1, false, false, false, true);
        }

        [Test]
        public void Test_GameString_NegativeAmountOfCall()
        {
            GameDefinition gameDef = LoadGameDef();
            string gameString = "1; jisully{0 1 0} syrpo{0 2 0}; 0d{5d 5s} 1d{Ts 8c} 0r2 1c d{2h 6h Tc} 1r2 0c d{5h} 1r4 0c d{As} 1r4 0r4 1c-3.6;";
            GameState gs = new GameState(gameString, gameDef);
            Assert.AreEqual(8, gs.Bet);
            Assert.AreEqual(32.4, gs.Pot);
            Assert.AreEqual(8, gs.Players[0].Bet);
            Assert.AreEqual(18, gs.Players[0].InPot);
            Assert.AreEqual(4.4, gs.Players[1].Bet);
            Assert.AreEqual(14.4, gs.Players[1].InPot);
        }

        [Test]
        public void Test_IsPlayerActing()
        {
            GameDefinition gameDef = LoadGameDef();
            GameState gs = new GameState(gameDef, 2);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.d(0, "Ac Ad"), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.d(1, "2c 3d"), gameDef);
            Assert.IsTrue(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsTrue(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.d("Ah As Qc"), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsTrue(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsTrue(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.r(0, 1), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsTrue(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.d("Jh"), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsTrue(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsTrue(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.d("Ts"), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsTrue(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsTrue(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            Assert.IsFalse(gs.IsPlayerActing(0));
            Assert.IsFalse(gs.IsPlayerActing(1));
            Assert.IsTrue(gs.IsGameOver);
        }

        [Test]
        public void Test_GameString_RemoveUnmatchedBet()
        {
            string gameString = "1; P1{0 .5 0} P2{0 1 0}; 0d{Ac Ad} 1d{Kc Kd} 0c 1r1 0f.";
            GameState gs = new GameState(gameString, null);
            Assert.AreEqual(1, gs.Players[0].InPot);
            Assert.AreEqual(2, gs.Players[1].InPot);
            Assert.AreEqual(3, gs.Pot);
            gs.RemoveUnmatchedBet();
            Assert.AreEqual(1, gs.Players[0].InPot);
            Assert.AreEqual(1, gs.Players[1].InPot);
            Assert.AreEqual(2, gs.Pot);

            gameString = "1; P1{0 .5 0} P2{0 1 0}; 0d{Ac Ad} 1d{Kc Kd} 0c 1r1 0c.";
            gs = new GameState(gameString, null);
            Assert.AreEqual(2, gs.Players[0].InPot);
            Assert.AreEqual(2, gs.Players[1].InPot);
            Assert.AreEqual(4, gs.Pot);
            gs.RemoveUnmatchedBet();
            Assert.AreEqual(2, gs.Players[0].InPot);
            Assert.AreEqual(2, gs.Players[1].InPot);
            Assert.AreEqual(4, gs.Pot);
        }

        [Test]
        public void Test_HasPlayerActed()
        {
            GameDefinition gameDef = LoadGameDef();
            GameState gs = new GameState(gameDef, 2);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.d(0, "Ac Ad"), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.d(1, "2c 3d"), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            Assert.IsTrue(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsTrue(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.d("Ah As Qc"), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsTrue(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.r(0, 1), gameDef);
            Assert.IsTrue(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsTrue(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.d("Jh"), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsTrue(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            Assert.IsTrue(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.d("Ts"), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(1), gameDef);
            Assert.IsFalse(gs.HasPlayerActed(0));
            Assert.IsTrue(gs.HasPlayerActed(1));

            gs.UpdateByAction(PokerAction.c(0), gameDef);
            Assert.IsTrue(gs.HasPlayerActed(0));
            Assert.IsFalse(gs.HasPlayerActed(1));
            Assert.IsTrue(gs.IsGameOver);
        }


        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_GameRecord_NoGameDef()
        {
            int repetitions = 1000000;
            Stopwatch sw = new Stopwatch();

            string gameString =
                "#133;FellOmen2{20 5 200} Probe{-20 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10 1c d{5c 8d 6s} 1c 0r10 1r10 0c d{Th} 1c 0r20 1r20 0c d{2s} 1c 0c.";

            GameRecord gr = new GameRecord(gameString);

            sw.Start();
            for (int i = 0; i < repetitions; ++i)
            {
                GameState gs = new GameState(gr, null);
            }
            sw.Stop();
            Console.WriteLine("{0} games in {1:#.##} s, {2:###,###,###} games/s, {3:###,###,###} actions/s",
                repetitions, sw.Elapsed.TotalSeconds, repetitions / sw.Elapsed.TotalSeconds,
                (double)gr.Actions.Count * repetitions / sw.Elapsed.TotalSeconds
                );
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_GameRecord_GameDef()
        {
            int repetitions = 1000000;
            GameDefinition gameDef = LoadGameDef();
            Stopwatch sw = new Stopwatch();

            string gameString =
                "#133;FellOmen2{20 5 200} Probe{-20 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10 1c d{5c 8d 6s} 1c 0r10 1r10 0c d{Th} 1c 0r20 1r20 0c d{2s} 1c 0c.";

            GameRecord gr = new GameRecord(gameString);

            sw.Start();
            for (int i = 0; i < repetitions; ++i)
            {
                GameState gs = new GameState(gr, gameDef);
            }
            sw.Stop();
            Console.WriteLine("{0} games in {1:#.##} s, {2:###,###,###} games/s, {3:###,###,###} actions/s",
                repetitions, sw.Elapsed.TotalSeconds, repetitions / sw.Elapsed.TotalSeconds,
                (double)gr.Actions.Count * repetitions / sw.Elapsed.TotalSeconds
                );
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_Clone()
        {
            int repetitions = 10000000;
            GameDefinition gameDef = LoadGameDef();
            Stopwatch sw = new Stopwatch();

            string gameString =
                "#133;FellOmen2{20 5 200} Probe{-20 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10 1c d{5c 8d 6s} 1c 0r10 1r10 0c d{Th} 1c 0r20 1r20 0c d{2s} 1c 0c.";

            GameRecord gr = new GameRecord(gameString);
            GameState gs1 = new GameState(gr, gameDef);

            sw.Start();
            for (int i = 0; i < repetitions; ++i)
            {
                GameState gs2 = new GameState(gs1);
            }
            sw.Stop();
            Console.WriteLine("{0} games in {1:#.##} s, {2:###,###,###} copies/s",
                repetitions, sw.Elapsed.TotalSeconds, repetitions / sw.Elapsed.TotalSeconds);
        }
        #endregion

        #region Implementation
        GameDefinition LoadGameDef()
        {
            string subdirName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            string testResourcesPath = Path.Combine(Props.Global.Get("bds.TestDir"), subdirName);
            GameDefinition gd;
            XmlSerializerExt.Deserialize(out gd, Path.Combine(testResourcesPath, "gamedef-test.xml"));
            return gd;
        }

        private void VerifyGameState(GameState gs, string Id, bool IsGameOver, int Round, double Pot, double Bet, int BetCount, int LastActor, bool IsDealerActing, int CurrentActor, int DealsCount, int FoldedCount, bool IsShowdownRequired, int Players_Length)
        {
            Assert.AreEqual(Id, gs.Id);
            Assert.AreEqual(IsGameOver, gs.IsGameOver);
            Assert.AreEqual(Round, gs.Round);
            Assert.AreEqual(Pot, gs.Pot);
            Assert.AreEqual(Bet, gs.Bet);
            Assert.AreEqual(BetCount, gs.BetCount);
            Assert.AreEqual(LastActor, gs.LastActor);
            Assert.AreEqual(IsDealerActing, gs.IsDealerActing);
            Assert.AreEqual(CurrentActor, gs.CurrentActor);
            Assert.AreEqual(DealsCount, gs.DealsCount);
            Assert.AreEqual(FoldedCount, gs.FoldedCount);
            Assert.AreEqual(IsShowdownRequired, gs.IsShowdownRequired);
            Assert.AreEqual(Players_Length, gs.Players.Length);
        }

        private void VerifyGameStateMethods(GameState gs, GameDefinition gameDef, Ak[] allowedActions)
        {
            Assert.AreEqual(allowedActions, gs.GetAllowedActions(gameDef));
        }

        private void VerifyPlayerState(PlayerState ps, string Name, double Stack, string Hand, double InPot, double Bet, double GameResult, bool IsFolded, bool IsAllIn, bool CanActInCurrentRound, bool CanActInGame)
        {
            Assert.AreEqual(Name, ps.Name);
            Assert.AreEqual(Stack, ps.Stack);
            Assert.AreEqual(Hand, ps.Hand);
            Assert.AreEqual(Bet, ps.Bet);
            Assert.AreEqual(InPot, ps.InPot);
            Assert.AreEqual(GameResult, ps.Result);
            Assert.AreEqual(IsFolded, ps.IsFolded);
            Assert.AreEqual(IsAllIn, ps.IsAllIn);
            Assert.AreEqual(CanActInCurrentRound, ps.CanActInCurrentRound);
            Assert.AreEqual(CanActInGame, ps.CanActInGame);
        }

        #endregion
    }
}
