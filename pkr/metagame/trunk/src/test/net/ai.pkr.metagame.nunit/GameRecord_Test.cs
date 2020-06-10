/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace ai.pkr.metagame.nunit
{
    [TestFixture]
    public class GameRecord_Test
    {
        #region Tests
        [Test]
        public void Test_ParseFinished()
        {
            string gameString =
                "125;FellOmen2{20 5 80} Probe{-40 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10 d{5c 8d 6s} 1c 0c d{Th} 1c 0c d{2s} 1c 0c.";

            string error = "";
            GameRecord gr;
            gr = GameRecord.Parse(gameString, out error);
            Assert.IsNotNull(gr);
            Assert.AreEqual("125", gr.Id);
            Assert.IsTrue(gr.IsGameOver);
            Assert.AreEqual(2, gr.Players.Count);
            Assert.AreEqual("FellOmen2", gr.Players[0].Name);
            Assert.AreEqual(20, gr.Players[0].Stack);
            Assert.AreEqual(5, gr.Players[0].Blind);
            Assert.AreEqual(80, gr.Players[0].Result);
            Assert.AreEqual("Probe", gr.Players[1].Name);
            Assert.AreEqual(-40, gr.Players[1].Stack);
            Assert.AreEqual(10, gr.Players[1].Blind);
            Assert.AreEqual(0, gr.Players[1].Result);
            Assert.AreEqual(14, gr.Actions.Count);
            VerifyActionRecord(0, Ak.d, "9s 8s", 0, gr.Actions[0]);
            VerifyActionRecord(1, Ak.d, "Qh Jd", 0, gr.Actions[1]);
            VerifyActionRecord(0, Ak.r, "", 10, gr.Actions[2]);
            VerifyActionRecord(1, Ak.r, "", 10, gr.Actions[3]);
            VerifyActionRecord(0, Ak.r, "", 10, gr.Actions[4]);
            VerifyActionRecord(-1, Ak.d, "5c 8d 6s", 0, gr.Actions[5]);
            VerifyActionRecord(1, Ak.c, "", 0, gr.Actions[6]);
            VerifyActionRecord(0, Ak.c, "", 0, gr.Actions[7]);
            VerifyActionRecord(-1, Ak.d, "Th", 0, gr.Actions[8]);
            VerifyActionRecord(1, Ak.c, "", 0, gr.Actions[9]);
            VerifyActionRecord(0, Ak.c, "", 0, gr.Actions[10]);
            VerifyActionRecord(-1, Ak.d, "2s", 0, gr.Actions[11]);
            VerifyActionRecord(1, Ak.c, "", 0, gr.Actions[12]);
            VerifyActionRecord(0, Ak.c, "", 0, gr.Actions[13]);
            
            // Make sure r at the end is parsed correctly (can be mixed with floating point)
            gameString = ";P0{20 5 80}; 0r123.";
            error = "";
            gr = GameRecord.Parse(gameString, out error);
            Assert.IsNotNull(gr);
            Assert.IsTrue(gr.IsGameOver);
            Assert.AreEqual(1, gr.Actions.Count);
            VerifyActionRecord(0, Ak.r, "", 123, gr.Actions[0]);

            // Check a negative amount of call
            gameString =
                "1; Pl1{174.2 1 13.4} Pl2{14.4 2 -14.4}; 0d{5d 5s} 1d{Ts 8c} 0r2 1c-3.6.";

            error = "";
            gr = GameRecord.Parse(gameString, out error);
            VerifyActionRecord(1, Ak.c, "", -3.6, gr.Actions[3]);
        }

        [Test]
        public void Test_ParseUnfinished()
        {
            // No Id, unfinished
            string gameString = ";FellOmen2{20.1 .5 0.0} Probe{-40.44 1.0 0};;";

            GameRecord gr = GameRecord.Parse(gameString);
            Assert.IsNotNull(gr);
            Assert.AreEqual("", gr.Id);
            Assert.IsFalse(gr.IsGameOver);
            Assert.AreEqual(2, gr.Players.Count);
            Assert.AreEqual("FellOmen2", gr.Players[0].Name);
            Assert.AreEqual(20.1, gr.Players[0].Stack);
            Assert.AreEqual(.5, gr.Players[0].Blind);
            Assert.AreEqual(0, gr.Players[0].Result);
            Assert.AreEqual("Probe", gr.Players[1].Name);
            Assert.AreEqual(-40.44, gr.Players[1].Stack);
            Assert.AreEqual(1.0, gr.Players[1].Blind);
            Assert.AreEqual(0, gr.Players[1].Result);
            Assert.AreEqual(0, gr.Actions.Count);
        }

        [Test]
        public void Test_ToString()
        {
            string gameString1 =
                "125; FellOmen2{20.5 0.5 80} Probe{-40 1.1 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10.5 0r10 d{5c 8d 6s} 1c 0c d{Th} 1c 0c d{2s} 1c 0c.";
            GameRecord gr = GameRecord.Parse(gameString1);
            string gameString2 = gr.ToGameString();
            Assert.AreEqual(gameString1, gameString2);
        }

        [Test]
        public void Test_NormalizeStakes()
        {
            GameRecord gr = new GameRecord("125; FellOmen2{20 5 80} Probe{-40 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10 d{5c 8d 6s} 1c 0c d{Th} 1c 0c d{2s} 1c 0c.");
            gr.NormalizeStakes(10);
            Assert.AreEqual(
                "125; FellOmen2{2 0.5 8} Probe{-4 1 0}; 0d{9s 8s} 1d{Qh Jd} 0r1 1r1 0r1 d{5c 8d 6s} 1c 0c d{Th} 1c 0c d{2s} 1c 0c.",
                gr.ToGameString());

        }

        [Test]
        [Category("Benchmark")]
        public void Test_ParseBenchmark()
        {
            int repetitions = 1000000;
            Stopwatch sw = new Stopwatch();

            string gameString =
                "#133;FellOmen2{20 5 200} Probe{-20 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10.5 1c d{5c 8d 6s} 1c 0r10 1r10 0c d{Th} 1c 0r20 1r20 0c d{2s} 1c 0c.";

            string error = "";
            sw.Start();
            for (int i = 0; i < repetitions; ++i)
            {
                if(GameRecord.Parse(gameString, out error) == null)
                {
                    Assert.Fail("Parsing failed: " + error);
                }
            }
            sw.Stop();
            Console.WriteLine("{0} game strings in {1:#.##} s, {2:#} gs/s", 
                repetitions, sw.Elapsed.TotalSeconds, repetitions / sw.Elapsed.TotalSeconds);
        }

        
        /// <summary>
        /// Random test.
        /// Algorithm:
        /// 1. Create a random game record gr1
        /// 2. Convert to string s1
        /// 3. Parse s1 to gr2
        /// 4. Convert gr2 to string s2
        /// 5. Compare s1 and s2.
        /// </summary>
        [Test]
        public void Test_Random()
        {
            int seed = (int)DateTime.Now.Ticks;
            Console.WriteLine("RNG seed {0}", seed);
            Random rng = new Random(seed);
            int repetitions = 20000;
            Ak[] actions = new Ak[] {Ak.d, Ak.r, Ak.c, Ak.f};
            for(int r = 0; r < repetitions; ++r)
            {
                GameRecord gr1 = new GameRecord();
                if (rng.Next(0, 2) == 0)
                {
                    gr1.Id = rng.Next().ToString();
                }
                gr1.IsGameOver = rng.Next(0, 2) == 0;

                int playerCount = rng.Next(1, 15);
                for(int p = 0; p < playerCount; ++p)
                {
                    GameRecord.Player player = new GameRecord.Player();
                    player.Name = "Pl_" + rng.NextDouble().ToString();
                    player.Stack = (rng.NextDouble() - 0.5)*1000;
                    player.Blind = rng.NextDouble()*1000;
                    player.Result = (rng.NextDouble() - 0.5)*1000;
                    gr1.Players.Add(player);
                }

                int actionCount = rng.Next(0, 100);
                for(int a = 0; a < actionCount; ++a)
                {
                    PokerAction action = new PokerAction();
                    action.Kind = actions[rng.Next(0, actions.Length)];
                    switch (action.Kind)
                    {
                        case Ak.d:
                            action.Cards = GenerateRandomCards(rng);
                            action.Position = rng.Next(-1, playerCount);
                            break;
                        case Ak.f:
                            action.Position = rng.Next(0, playerCount);
                            break;
                        case Ak.c:
                            action.Position = rng.Next(0, playerCount);
                            if (rng.Next(0, 2) == 0)
                            {
                                action.Amount = -rng.NextDouble() * 1000;
                            }
                            break;
                        case Ak.r:
                            action.Position = rng.Next(0, playerCount);
                            action.Amount = rng.NextDouble() * 1000;
                            break;
                        default:
                            Assert.Fail("Unexpected action");
                            break;
                    }
                    gr1.Actions.Add(action);
                }
                string gs1 = gr1.ToGameString();
                GameRecord gr2 = new GameRecord(gs1);
                string gs2 = gr2.ToGameString();
                Assert.AreEqual(gs1, gs2);
            }
        }

        private string GenerateRandomCards(Random rng)
        {
            string cards = "";
            int cardsCnt = rng.Next(1, 10);
            for (int c = 0; c < cardsCnt; ++c)
            {
                if (c > 0)
                    cards += " ";
                cards += rng.Next(1, 100).ToString();
            }
            return cards;
        }

        #endregion

        #region Implementation

        private void VerifyActionRecord(int pos, Ak kind, string cards, double amount, PokerAction ar)
        {
            Assert.AreEqual(pos, ar.Position);
            Assert.AreEqual(kind, ar.Kind);
            Assert.AreEqual(cards, ar.Cards);
            Assert.AreEqual(amount, ar.Amount);
        }

        #endregion
    }
}
