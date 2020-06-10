/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;

namespace ai.pkr.metatools.nunit
{
    /// <summary>
    /// Unit tests for TransformGameRecords. 
    /// </summary>
    [TestFixture]
    public class TransformGameRecords_Test
    {
        #region Tests

        [Test]
        public void Test_FinalizeGames()
        {
            TransformGameRecords tr = new TransformGameRecords { FinalizeGames = true };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1;");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1.", gr.ToGameString()); 
        }

        [Test]
        public void Test_RenumerateGames()
        {
            TransformGameRecords tr = new TransformGameRecords { RenumerateGames = true, GameCount = 5, RemoveNoShowdown = true };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("5; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
            gr = new GameRecord("16; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0r1 1f.");
            Assert.IsFalse(tr.Transform(gr));
            gr = new GameRecord("18; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac 3s} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("6; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac 3s} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_HideOpponentCards()
        {
            TransformGameRecords tr = new TransformGameRecords { HideOpponentCards = true, HeroName = "Agent" };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{? ?} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_NormalizeCards()
        {
            TransformGameRecords tr = new TransformGameRecords { NormalizeCards = true };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac  Ad } 1d{ Kc 2d } 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_ResetStacks()
        {
            TransformGameRecords tr = new TransformGameRecords { ResetStacks = true};
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{0 0.5 3} Opp{0 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_ResetResults()
        {
            TransformGameRecords tr = new TransformGameRecords { ResetResults = true };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{10 0.5 0} Opp{15 1 0}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_NormalizeStakes()
        {
            TransformGameRecords tr = new TransformGameRecords { NormalizeStakes = 5 };
            GameRecord gr = new GameRecord("13; Agent{100 5 30} Opp{150 10 -30}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r10 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{20 1 6} Opp{30 2 -6}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r2 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());

            tr = new TransformGameRecords { NormalizeStakes = 0 };
            gr = new GameRecord("13; Agent{100 5 30} Opp{150 10 -30}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r10 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_RenameEq()
        {
            TransformGameRecords tr = new TransformGameRecords { RenameEqName = "Agent", RenameEqNewName = "Bot" };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Bot{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_RenameNeq()
        {
            TransformGameRecords tr = new TransformGameRecords { RenameNeqName = "Agent", RenameNeqNewName = "Enemy" };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            Assert.AreEqual("13; Agent{10 0.5 3} Enemy{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.", gr.ToGameString());
        }

        [Test]
        public void Test_RemoveNoHeroMoves()
        {
            TransformGameRecords tr = new TransformGameRecords { RemoveNoHeroMoves = true, HeroName = "Agent"};
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            gr = new GameRecord("16; Opp{10 0.5 3} Agent{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0f.");
            Assert.IsFalse(tr.Transform(gr));
        }

        [Test]
        public void Test_RemoveNoShowdown()
        {
            TransformGameRecords tr = new TransformGameRecords { RemoveNoShowdown = true };
            GameRecord gr = new GameRecord("13; Agent{10 0.5 3} Opp{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0c 1r1 d{2h 3h 4h} 1c 0c d{Jh} 1c 0c d{Qd} 1c 0c.");
            Assert.IsTrue(tr.Transform(gr));
            gr = new GameRecord("16; Opp{10 0.5 3} Agent{15 1 -3}; 0d{Ac Ad} 1d{Kc 2d} 0r1 1r1 0f.");
            Assert.IsFalse(tr.Transform(gr));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
