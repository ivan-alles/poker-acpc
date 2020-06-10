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
    /// Unit tests for HandValue. 
    /// </summary>
    [TestFixture]
    public class HandValue_Test
    {
        #region Tests

        [Test]
        public void Test_ValueToString()
        {
            CardSet m;
            UInt32 value;
            string text;

            m = StdDeck.Descriptor.GetCardSet("2s 3h 6c Ah 9d Kd Js");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("High Card: Ah Kd Js 9d 6c", text);

            m = StdDeck.Descriptor.GetCardSet("7s 6h 7c Ah 9d Qd Jd");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("Pair: 7c 7s Ah Qd Jd", text);

            m = StdDeck.Descriptor.GetCardSet("7s 6h 7c Ah 9d Qd Ad");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("2 Pair: Ad Ah 7c 7s Qd", text);

            m = StdDeck.Descriptor.GetCardSet("2s 6h 7c Ah 2d Qd 2c");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("3 of a Kind: 2c 2d 2s Ah Qd", text);

            m = StdDeck.Descriptor.GetCardSet("2s 6h 3c 5h 4d 8d 2c");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("Straight: 6h 5h 4d 3c 2c", text);

            m = StdDeck.Descriptor.GetCardSet("2s Ah 3c 5h 4d 8d 2c");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("Straight: 5h 4d 3c 2c Ah", text);

            m = StdDeck.Descriptor.GetCardSet("2s 6s Qs 4h 8s 8d 7s");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("Flush: Qs 8s 7s 6s 2s", text);

            m = StdDeck.Descriptor.GetCardSet("2s 6h 7c Ah 2d Qd 2c");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("3 of a Kind: 2c 2d 2s Ah Qd", text);

            m = StdDeck.Descriptor.GetCardSet("2s 6h 7c 6c 2d Qd 2c");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("Full House: 2c 2d 2s 6c 6h", text);

            m = StdDeck.Descriptor.GetCardSet("2s 6h 7c Ah 2d 2h 2c");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("4 of a Kind: 2c 2d 2h 2s Ah", text);

            m = StdDeck.Descriptor.GetCardSet("As 5s 9s 7s 8s 6s Qc");
            value = CardSetEvaluator.Evaluate(ref m);
            text = HandValue.ValueToString(m, value);
            Assert.AreEqual("Straight Flush: 9s 8s 7s 6s 5s", text);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
