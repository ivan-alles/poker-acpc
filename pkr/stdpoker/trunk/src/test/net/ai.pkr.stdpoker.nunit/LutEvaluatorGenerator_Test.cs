/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;

namespace ai.pkr.stdpoker.nunit
{
    /// <summary>
    /// Unit tests for LutEvaluatorGenerator. 
    /// Most of the tests are set to excplicit because 
    /// 1) they run very long, so it cannot be tested cheap
    /// 2) we need to test the generated LUT (and we do it elsewhere).
    /// So, it's more or less preliminary testing to quickly verify the algo 
    /// after some changes in code.
    /// </summary>
    [TestFixture]
    public class LutEvaluatorGenerator_Test
    {
        #region Tests

        [Test]
        public void Test_ExtractFlush()
        {
            CardSet c, f, exp;

            c = StdDeck.Descriptor.GetCardSet("2s");
            f = LutEvaluatorGenerator.ExtractFlush(c, 5);
            exp = StdDeck.Descriptor.GetCardSet("2s");
            Assert.AreEqual(exp, f);


            c = StdDeck.Descriptor.GetCardSet("3c 2c");
            f = LutEvaluatorGenerator.ExtractFlush(c, 5);
            exp = StdDeck.Descriptor.GetCardSet("3c 2c");
            Assert.AreEqual(exp, f);
        }

        [Test]
        [Explicit]
        public void Test_5_Hands()
        {
            LutEvaluatorGenerator g = new LutEvaluatorGenerator();
            g.GenerateStates(5);
            Assert.IsTrue(g.Test5Hands());
        }

        [Test]
        [Explicit]
        public void Test_6_Hands()
        {
            LutEvaluatorGenerator g = new LutEvaluatorGenerator();
            g.GenerateStates(6);
            Assert.IsTrue(g.Test6Hands());
        }

        [Test]
        [Explicit]
        public void Test_7_Hands()
        {
            LutEvaluatorGenerator g = new LutEvaluatorGenerator();
            g.GenerateStates(7);
            Assert.IsTrue(g.Test7Hands());
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
