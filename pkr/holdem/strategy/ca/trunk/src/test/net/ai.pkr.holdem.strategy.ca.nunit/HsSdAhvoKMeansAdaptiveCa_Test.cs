/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ai.lib.algorithms.tree;
using ai.lib.utils;
using ai.pkr.metagame;
using ai.pkr.holdem.strategy.hs;
using System.Reflection;
using System.Globalization;

namespace ai.pkr.holdem.strategy.ca.nunit
{
    /// <summary>
    /// Unit tests for HsSdAhvoKMeansAdaptiveCa. 
    /// As it is very difficult to verify results, the following method is used for most of the tests:
    /// A CA was generated manually and some selected hands were checked using GNU plot and logs.
    /// The same abstraction is genereated in the UT and the values of these hands are verified.
    /// </summary>
    [TestFixture]
    public class HsSdAhvoKMeansAdaptiveCa_Test
    {
        #region Tests

        [Test]
        public void Test_GetAbstractCard()
        {
            Props parameters = XmlSerializerExt.Deserialize<Props>(Path.Combine(_testResDir, "ca-hssd-ahvo-km.xml"));
            HsSdAhvoKMeansAdaptiveCa ca = CalculateCa(parameters, new int[] { 0, 5000, 5000, 5000 }, 1);
            VerifyPreflopPockets(ca);

            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;

            // In comments there are normalized values to verify in debugger.

            #region Preflop bucket 7 (AA)

            //------------------------------------------------------------
            //	0.61537	0.42559	0.09124
            hand = dd.GetIndexes("Ac As Kh 5h 2d");
            Assert.AreEqual(3, ca.GetAbstractCard(hand, hand.Length));

            //	0.63778	0.43364	0.35852
            hand = dd.GetIndexes("Ad As 7s 7h 5s");
            Assert.AreEqual(4, ca.GetAbstractCard(hand, hand.Length));

            #endregion

            #region Preflop bucket 7 (AA), flop bucket 4

            hand = dd.GetIndexes("Ac Ah Td Jh Js 5h");
            Assert.AreEqual(4, ca.GetAbstractCard(hand, hand.Length - 1));
            // 0.647707503 0.222747622 0.335863970
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("Ad Ah 5d 5s Ks 2d");
            Assert.AreEqual(4, ca.GetAbstractCard(hand, hand.Length - 1));
            // 0.642999891 0.228836240 0.187371225
            Assert.AreEqual(7, ca.GetAbstractCard(hand, hand.Length));

            #endregion

            #region Preflop bucket 7 (AA), flop bucket 4, turn bucket 6

            hand = dd.GetIndexes("Ac Ad 2h Qd Qh 3s 2s");
            Assert.AreEqual(4, ca.GetAbstractCard(hand, hand.Length - 2));
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length - 1));
            // 0.20689665	0.00000000	0.44819598
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("Ad Ah Qd Th Td 7c Jd");
            Assert.AreEqual(4, ca.GetAbstractCard(hand, hand.Length - 2));
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length - 1));
            // 0.22068975	0.00000000	0.16117185
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            #endregion

        }

        #endregion


        #region Benchmarks
        #endregion

        #region Implementation


        void VerifyPreflopPockets(HsSdAhvoKMeansAdaptiveCa ca)
        {
            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;

            hand = dd.GetIndexes("Ac Ah");
            Assert.AreEqual(7, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("Ac Kc");
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("Ac Kh");
            Assert.AreEqual(5, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("5s 5h");
            Assert.AreEqual(4, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("7s 8s");
            Assert.AreEqual(3, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("8s 7d");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("7c 2d");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));

            hand = dd.GetIndexes("Qc 2d");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));
        }


        HsSdAhvoKMeansAdaptiveCa CalculateCa(Props parameters, int[] samplesCount, int rngSeed)
        {
            ClusterTree rt = new ClusterTree();
            parameters.Set("IsCreatingClusterTree", "true");
            HsSdAhvoKMeansAdaptiveCa ca = new HsSdAhvoKMeansAdaptiveCa(parameters);

            CaMcGen gen = new CaMcGen
            {
                Clusterizer = ca,
                IsVerbose = false,
                // IsVerboseSamples = true,
                RngSeed = rngSeed,
                SamplesCount = samplesCount
            };
            rt.Root = gen.Generate();
            string fileName = Path.Combine(_outDir, "ca-hssd-ahvo-km.dat");
            rt.Write(fileName);

            parameters.Set("ClusterTreeFile", fileName);
            parameters.Set("IsCreatingClusterTree", "false");
            HsSdAhvoKMeansAdaptiveCa ca1 = new HsSdAhvoKMeansAdaptiveCa(parameters);
            return ca1;
        }

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "HsSdAhvoKMeansAdaptiveCa_Test");
        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());

        #endregion
    }
}
