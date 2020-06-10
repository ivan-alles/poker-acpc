/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.holdem.strategy.core.nunit
{
    [TestFixture]
    public class HePocket_Test
    {
        #region Tests
        [Test]
        public void Test_CardSetToKind()
        {
            Assert.AreEqual(169, (int)HePocketKind.__Count);
            DeckDescriptor deck = StdDeck.Descriptor;
            Assert.AreEqual(HePocketKind._AQo, HePocket.CardSetToKind(deck.GetCardSet("Ac Qd")));
            Assert.AreEqual(HePocketKind._AQo, HePocket.CardSetToKind(deck.GetCardSet("As Qc")));
            Assert.AreEqual(HePocketKind._22, HePocket.CardSetToKind(deck.GetCardSet("2s 2c")));
            Assert.AreEqual(HePocketKind._32s, HePocket.CardSetToKind(deck.GetCardSet("2s 3s")));
            Assert.AreEqual(HePocketKind._32s, HePocket.CardSetToKind(deck.GetCardSet("3c 2c")));
        }

        [Test]
        public void Test_KindToCardset()
        {
            Assert.AreEqual(169, (int)HePocketKind.__Count);
            DeckDescriptor deck = StdDeck.Descriptor;
            Assert.AreEqual(deck.GetCardSet("Ac Ad"), HePocket.KindToCardSet(HePocketKind._AA));
            Assert.AreEqual(deck.GetCardSet("Ac Kd"), HePocket.KindToCardSet(HePocketKind._AKo));
            Assert.AreEqual(deck.GetCardSet("Ac Kc"), HePocket.KindToCardSet(HePocketKind._AKs));
            Assert.AreEqual(deck.GetCardSet("7c 2d"), HePocket.KindToCardSet(HePocketKind._72o));
            Assert.AreEqual(deck.GetCardSet("7c 5c"), HePocket.KindToCardSet(HePocketKind._75s));

            //for (int i = 0; i < 169; ++i)
            //{
            //    HePocketKind pk = (HePocketKind)i;
            //    Console.WriteLine("{0,-4}: {1}", pk, HePocket.KindToCardSet(pk));
            //}
        }

        [Test]
        public void Test_HandToToKind()
        {
            DeckDescriptor deck = StdDeck.Descriptor;
            Assert.AreEqual(HePocketKind._AQo, HePocket.HandToKind(deck.GetIndexes("Ac Qd")));
            Assert.AreEqual(HePocketKind._AQo, HePocket.HandToKind(deck.GetIndexes("As Qc")));
            Assert.AreEqual(HePocketKind._22, HePocket.HandToKind(deck.GetIndexes("2s 2c")));
            Assert.AreEqual(HePocketKind._32s, HePocket.HandToKind(deck.GetIndexes("2s 3s")));
            Assert.AreEqual(HePocketKind._32s, HePocket.HandToKind(deck.GetIndexes("3c 2c")));
        }

        [Test]
        public void Test_KindToHand()
        {
            DeckDescriptor deck = StdDeck.Descriptor;
            int[] hand = HePocket.KindToHand(HePocketKind._AA);
            Assert.AreEqual(deck.GetIndexes("Ac Ad").ToArray(), hand);
            hand = HePocket.KindToHand(HePocketKind._KQo);
            Assert.AreEqual(deck.GetIndexes("Kc Qd").ToArray(), hand);
            hand = HePocket.KindToHand(HePocketKind._54s);
            Assert.AreEqual(deck.GetIndexes("4c 5c").ToArray(), hand);
        }

        /// <summary>
        /// Cross-tests KindToString() and StringToKind().
        /// </summary>
        [Test]
        public void Test_KindToString_StringToKind()
        {
            for (int p = 0; p < (int)HePocketKind.__Count; ++p)
            {
                HePocketKind kind = (HePocketKind) p;
                string kindString = HePocket.KindToString(kind);
                HePocketKind kind1 = HePocket.StringToKind(kindString);
                Assert.AreEqual(kind, kind1);
            }
        }

        /// <summary>
        /// Cross-tests multiple functions.
        /// </summary>
        [Test]
        public void Test_KindToRange_CardSetToKind_KindToSuiteNormalizedCardSet()
        {
            HashSet<CardSet> uniquePockets = new HashSet<CardSet>();
            for (int p = 0; p < (int)HePocketKind.__Count; ++p)
            {
                HePocketKind kind = (HePocketKind)p;
                CardSet[] range = HePocket.KindToRange(kind);
                CardSet ncsExp = HePocket.KindToCardSet(kind);
                foreach (CardSet cs in range)
                {
                    Assert.AreEqual(kind, HePocket.CardSetToKind(cs), "Each pocket from range must be of the expected kind.");
                    NormSuit ns = new NormSuit();
                    CardSet ncs = ns.Convert(cs);
                    Assert.AreEqual(ncsExp, ncs, "Card set from ranges must transform to same normalized card set");

                    // This will throw an exception if some pockets were duplicated.
                    uniquePockets.Add(cs);
                }
            }
            Assert.AreEqual(1326, uniquePockets.Count);
        }
        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_CardSetToKind()
        {
            CardSet[] pockets = CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, CardSet.Empty);
            int repetitions = 100;
            DateTime startTime = DateTime.Now;

            int checksum = 0;

            for (int r = 0; r < repetitions; ++r)
            {
                for (int p = 0; p < pockets.Length; ++p)
                {
                    checksum += (int)HePocket.CardSetToKind(pockets[p]);
                }
            }

            double runTime = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine("Cardset to kind: count: {0:#,#}, {1:#,#} r/s, checksum: {2}",
                repetitions * pockets.Length, repetitions * pockets.Length / runTime, checksum);

            startTime = DateTime.Now;

            CardSet checksum1 = CardSet.Empty;

            NormSuit ns = new NormSuit();
            for (int r = 0; r < repetitions; ++r)
            {
                for (int p = 0; p < pockets.Length; ++p)
                {
                    checksum1 |= ns.Convert(pockets[p]);
                    ns.Reset();
                }
            }

            runTime = (DateTime.Now - startTime).TotalSeconds;

            Console.WriteLine("To compare performance:");

            Console.WriteLine("Normalize suit : count: {0:#,#}, {1:#,#} r/s, checksum: {2}",
                repetitions * pockets.Length, repetitions * pockets.Length / runTime, checksum1.bits);

        }

        #endregion
    }
}
