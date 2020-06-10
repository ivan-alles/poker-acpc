/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy.algorithms;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.pkr.metastrategy.nunit;

namespace ai.pkr.metastrategy.algorithms.nunit
{
    /// <summary>
    /// Unit tests for NormRank. 
    /// </summary>
    [TestFixture]
    public class NormRank_Test
    {
        #region Tests

        [Test]
        public void Test_Convert()
        {
            DeckDescriptor dd = StdDeck.Descriptor;
            Assert.AreEqual(CardSet.Empty, NormRank.Convert(CardSet.Empty));
            Assert.AreEqual(dd.FullDeck, NormRank.Convert(dd.FullDeck));
            CardSet cs = new CardSet { bits = 0xFFFFFFFFFFFFFFFF};
            Assert.AreEqual(cs, NormRank.Convert(cs));
            
            Assert.AreEqual(dd.GetCardSet("Ac"), NormRank.Convert(dd.GetCardSet("Ac")));
            Assert.AreEqual(dd.GetCardSet("Ac"), NormRank.Convert(dd.GetCardSet("Ad")));
            Assert.AreEqual(dd.GetCardSet("Ac"), NormRank.Convert(dd.GetCardSet("Ah")));
            Assert.AreEqual(dd.GetCardSet("Ac"), NormRank.Convert(dd.GetCardSet("As")));

            Assert.AreEqual(dd.GetCardSet("Ac Kc"), NormRank.Convert(dd.GetCardSet("Ac Kc")));
            Assert.AreEqual(dd.GetCardSet("Ac Kc"), NormRank.Convert(dd.GetCardSet("Ac Kd")));
            Assert.AreEqual(dd.GetCardSet("Ac Kc"), NormRank.Convert(dd.GetCardSet("Ad Kc")));
            Assert.AreEqual(dd.GetCardSet("Ac Kc"), NormRank.Convert(dd.GetCardSet("As Kh")));

            Assert.AreEqual(dd.GetCardSet("3c 3d"), NormRank.Convert(dd.GetCardSet("3c 3d")));
            Assert.AreEqual(dd.GetCardSet("3c 3d"), NormRank.Convert(dd.GetCardSet("3c 3h")));
            Assert.AreEqual(dd.GetCardSet("3c 3d"), NormRank.Convert(dd.GetCardSet("3s 3h")));

            Assert.AreEqual(dd.GetCardSet("2c 3c 3d 4c 4d 4h 6c 6d 6h 6s"), 
                NormRank.Convert(dd.GetCardSet("2d 3s 3h 4c 4h 4d 6s 6d 6h 6c")));
        }

        [Test]
        public void Test_CountEquiv()
        {
            DeckDescriptor dd = StdDeck.Descriptor;
            Assert.AreEqual(1, NormRank.CountEquiv(CardSet.Empty, dd));
            Assert.AreEqual(4 * 4 * 4 * 4, NormRank.CountEquiv(dd.GetCardSet("Ac Kd Qc Jh"), dd));
            Assert.AreEqual(16, NormRank.CountEquiv(dd.GetCardSet("Ac Kd"), dd));
            Assert.AreEqual(4, NormRank.CountEquiv(dd.GetCardSet("Kd"), dd));
            Assert.AreEqual(6, NormRank.CountEquiv(dd.GetCardSet("Kc Kd"), dd));
            Assert.AreEqual(4, NormRank.CountEquiv(dd.GetCardSet("Kc Kd Ks"), dd));
            Assert.AreEqual(1, NormRank.CountEquiv(dd.GetCardSet("7c 7d 7s 7h"), dd));
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
