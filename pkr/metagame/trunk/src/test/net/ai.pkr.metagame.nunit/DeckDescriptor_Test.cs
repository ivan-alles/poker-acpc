/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ai.lib.utils;
using System.IO;

namespace ai.pkr.metagame.nunit
{
    [TestFixture]
    public class DeckDescriptor_Test
    {
        #region Tests
        string[] _cardNames1 = new string[] {"Q", "K", "A"};

        private CardSet[] _cardSets1 = new CardSet[]
                                         {
                                             new CardSet {bits = 0x01}, 
                                             new CardSet {bits = 0x02},
                                             new CardSet {bits = 0x10}
                                         };

        [Test]
        public void Test_Properties()
        {
            DeckDescriptor dd = new DeckDescriptor("TestDeck", _cardNames1, _cardSets1);
            Assert.AreEqual("TestDeck", dd.Name);
            Assert.AreEqual(_cardNames1.Length, dd.Size);
            Assert.AreEqual(_cardNames1, dd.CardNames);
            Assert.AreEqual(_cardSets1, dd.CardSets);
            Assert.AreEqual(0x13, dd.FullDeck.bits);
            Assert.AreEqual(new int[] { 0, 1, 2 }, dd.FullDeckIndexes);
            Assert.IsFalse(dd.HasDuplicates);
        }

        [Test]
        public void Test_HasDuplicates()
        {
            DeckDescriptor dd = new DeckDescriptor("TestDeck", new string[] { "Q", "K", "A" }, _cardSets1);
            Assert.IsFalse(dd.HasDuplicates);
            dd = new DeckDescriptor("TestDeck", new string[] { "Q", "Q", "A" }, _cardSets1);
            Assert.IsTrue(dd.HasDuplicates);
        }

        [Test]
        public void Test_Indexes()
        {
            DeckDescriptor dd = new DeckDescriptor("TestDeck", _cardNames1, _cardSets1);
            Assert.AreEqual(0, dd.GetIndex("Q"));
            Assert.AreEqual(1, dd.GetIndex("K"));
            Assert.AreEqual(2, dd.GetIndex("A"));

            Assert.AreEqual(new int[] {}, dd.GetIndexes(""));
            Assert.AreEqual(new int[] { 2 }, dd.GetIndexes("A"));
            Assert.AreEqual(new int[] { 1, 2 }, dd.GetIndexes("K A"));
            Assert.AreEqual(new int[] { 2, 1 }, dd.GetIndexes("A K"));

            Assert.AreEqual(new List<int>(), dd.GetIndexesAscending(new CardSet { bits = 0x0 }));
            Assert.AreEqual(new List<int>(new int[] { 0 }), dd.GetIndexesAscending(new CardSet { bits = 0x1 }));
            Assert.AreEqual(new List<int>(new int[] { 0, 2 }), dd.GetIndexesAscending(new CardSet { bits = 0x11 }));
        }

        [Test]
        public void Test_Cardsets()
        {
            DeckDescriptor dd = new DeckDescriptor("TestDeck", _cardNames1, _cardSets1);

            Assert.AreEqual(0x00, dd.GetCardSet("").bits);
            Assert.AreEqual(0x00, dd.GetCardSet(" ").bits);
            Assert.AreEqual(0x01, dd.GetCardSet("Q").bits);
            Assert.AreEqual(0x01, dd.GetCardSet("Q ").bits);
            Assert.AreEqual(0x01, dd.GetCardSet(" Q").bits);
            Assert.AreEqual(0x01, dd.GetCardSet(" Q ").bits);
            Assert.AreEqual(0x11, dd.GetCardSet("Q A").bits);
            Assert.AreEqual(0x11, dd.GetCardSet("A Q").bits);
            Assert.AreEqual(0x11, dd.GetCardSet(" Q A").bits);
            Assert.AreEqual(0x11, dd.GetCardSet("A   Q").bits);
            Assert.AreEqual(0x11, dd.GetCardSet("A   Q ").bits);


            Assert.AreEqual(0x00, dd.GetCardSet(new string[]{}).bits);
            Assert.AreEqual(0x01, dd.GetCardSet(new string[]{"Q"}).bits);
            Assert.AreEqual(0x11, dd.GetCardSet(new string[]{"Q", "A"}).bits);
            Assert.AreEqual(0x11, dd.GetCardSet(new string[]{"A", "Q"}).bits);

            Assert.AreEqual(0x00, dd.GetCardSet(new int[] { }).bits);
            Assert.AreEqual(0x13, dd.GetCardSet(new int[] { 0, 2, 1 }).bits);
            Assert.AreEqual(0x13, dd.GetCardSet(new int[] { 1, 2, 0 }).bits);

            Assert.AreEqual(0x00, dd.GetCardSet(new int[] { 0, 2, 1 }, 1, 0).bits);
            Assert.AreEqual(0x12, dd.GetCardSet(new int[] { 0, 2, 1 }, 1, 2).bits);
            Assert.AreEqual(0x12, dd.GetCardSet(new int[] { 0, 1, 2 }, 1, 2).bits);
        }

        [Test]
        public void Test_Strings()
        {
            DeckDescriptor dd = new DeckDescriptor("TestDeck", _cardNames1, _cardSets1);

            Assert.AreEqual("", dd.GetCardNames(new CardSet { bits = 0x00 }));
            Assert.AreEqual("A Q", dd.GetCardNames(new CardSet { bits = 0x11 }));
            Assert.AreEqual("A K Q", dd.GetCardNames(new CardSet { bits = 0x13 }));


            Assert.AreEqual(new string[]{}, dd.GetCardNamesArray(new CardSet { bits = 0x00 }));
            Assert.AreEqual(new string[]{"A", "Q"}, dd.GetCardNamesArray(new CardSet { bits = 0x11 }));
            Assert.AreEqual(new string[]{"A", "K", "Q"}, dd.GetCardNamesArray(new CardSet { bits = 0x13 }));

            Assert.AreEqual(new string[] { }, dd.GetCardNamesArray(new int[] { }));
            Assert.AreEqual(new string[] { "A", "K" }, dd.GetCardNamesArray(new int[] { 2, 1 }));
            Assert.AreEqual(new string[] { "A", "K", "Q" }, dd.GetCardNamesArray(new int[] { 2, 1, 0 }));

            Assert.AreEqual(new string[] { }, dd.GetCardNamesArray(new int[] { 2, 1, 0}, 1, 0));
            Assert.AreEqual(new string[] { "K" }, dd.GetCardNamesArray(new int[] { 2, 1 }, 1, 1));
            Assert.AreEqual(new string[] { "A", "K" }, dd.GetCardNamesArray(new int[] { 2, 1, 0 }, 0, 2));

        }

        [Test]
        public void Test_XmlSerialization()
        {
            DeckDescriptor dd = new DeckDescriptor("TestDeck", _cardNames1, _cardSets1);

            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                dd.XmlSerialize(tw);
            }

            Console.WriteLine(sb.ToString());

            DeckDescriptor dd2;

            using (TextReader textReader = new StringReader(sb.ToString()))
            {
                dd2 = XmlSerializerExt.Deserialize<DeckDescriptor>(textReader);
            }

            dd2.ConstructFromXml(null);

            Assert.IsNotNull(dd2);

            Assert.AreEqual(dd.Name, dd2.Name);
            Assert.AreEqual(dd.Size, dd2.Size);
            Assert.AreEqual(dd.CardNames, dd2.CardNames);
            Assert.AreEqual(dd.CardSets, dd2.CardSets);
            Assert.AreEqual(dd.FullDeck, dd2.FullDeck);
            Assert.AreEqual(dd.FullDeckIndexes, dd2.FullDeckIndexes);
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_GetIndex()
        {
            DeckDescriptor dd = StdDeck.Descriptor;

            int repetitions = 10000000;
            DateTime start = DateTime.Now;

            int sum = 0;
            for(int r = 0; r < repetitions; ++r)
            {
                sum += dd.GetIndex(dd.CardNames[r%dd.Size]);
            }

            double time = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine("Repetitions: {0:#,#}, time: {1:0.00} s, rep/s: {2:#,#}",
                repetitions, time, repetitions / time);
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_GetCardSet()
        {
            DeckDescriptor dd = StdDeck.Descriptor;

            int repetitions = 10000000;
            DateTime start = DateTime.Now;

            UInt64 sum = 0;
            for (int r = 0; r < repetitions; ++r)
            {
                sum += dd.GetCardSet(dd.CardNames[r % dd.Size]).bits;
            }

            double time = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine("Repetitions: {0:#,#}, time: {1:0.00} s, rep/s: {2:#,#}",
                repetitions, time, repetitions / time);
        }

        #endregion
    }
}
