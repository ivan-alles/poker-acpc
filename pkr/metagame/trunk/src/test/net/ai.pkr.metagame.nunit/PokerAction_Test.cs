/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.metagame.nunit
{
    /// <summary>
    /// Unit tests for PokerAction. 
    /// </summary>
    [TestFixture]
    public class PokerAction_Test
    {
        #region Tests

        [Test]
        public void Test_IsPlayerAction()
        {
            PokerAction action = new PokerAction();
            action.Kind = Ak.b;
            Assert.IsFalse(action.IsPlayerAction());
            action.Kind = Ak.f;
            Assert.IsTrue(action.IsPlayerAction());
            action.Kind = Ak.c;
            Assert.IsTrue(action.IsPlayerAction());
            action.Kind = Ak.r;
            Assert.IsTrue(action.IsPlayerAction());
            action.Kind = Ak.d;
            Assert.IsFalse(action.IsPlayerAction());
        }

        [Test]
        public void Test_IsPlayerAction_Static()
        {
            Assert.IsFalse(PokerAction.IsPlayerAction(Ak.b));
            Assert.IsTrue(PokerAction.IsPlayerAction(Ak.f));
            Assert.IsTrue(PokerAction.IsPlayerAction(Ak.c));
            Assert.IsTrue(PokerAction.IsPlayerAction(Ak.r));
            Assert.IsFalse(PokerAction.IsPlayerAction(Ak.d));
        }

        [Test]
        public void Test_IsDealerAction()
        {
            PokerAction action = new PokerAction();
            action.Kind = Ak.b;
            Assert.IsFalse(action.IsDealerAction());
            action.Kind = Ak.f;
            Assert.IsFalse(action.IsDealerAction());
            action.Kind = Ak.c;
            Assert.IsFalse(action.IsDealerAction());
            action.Kind = Ak.r;
            Assert.IsFalse(action.IsDealerAction());
            action.Kind = Ak.d;
            Assert.IsTrue(action.IsDealerAction());
        }

        [Test]
        public void Test_IsDealerAction_Static()
        {
            Assert.IsFalse(PokerAction.IsDealerAction(Ak.b));
            Assert.IsFalse(PokerAction.IsDealerAction(Ak.f));
            Assert.IsFalse(PokerAction.IsDealerAction(Ak.c));
            Assert.IsFalse(PokerAction.IsDealerAction(Ak.r));
            Assert.IsTrue(PokerAction.IsDealerAction(Ak.d));
        }


        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
