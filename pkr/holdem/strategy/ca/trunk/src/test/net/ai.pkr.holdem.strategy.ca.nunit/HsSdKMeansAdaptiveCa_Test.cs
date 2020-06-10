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
    /// Unit tests for HsSdKMeansAdaptiveCa (tests indirectly also KMeansAdaptiveCaBase). 
    /// As it is very difficult to verify results, the following method is used for most of the tests:
    /// A CA was generated manually and some selected hands were checked using GNU plot and logs.
    /// The same abstraction is genereated in the UT and the values of these hands are verified.
    /// </summary>
    [TestFixture]
    public class HsSdKMeansAdaptiveCa_Test
    {
        #region Tests

        [Test]
        public void Test_SD3_Normalized()
        {
            Props parameters = XmlSerializerExt.Deserialize<Props>(Path.Combine(_testResDir, "ca-hssd-km.xml"));

            parameters.Set("SdKind", "Sd3");
            parameters.Set("NormalizeHandValues", "true");

            HsSdKMeansAdaptiveCa ca = CalculateCa(parameters, new int[] { 0, 5000, 5000, 200 }, 1);
            VerifyPreflopPockets(ca);

            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;

            // In comments there are normalized values.

            #region Preflop bucket 3 (87s)
            //------------------------------------------------------------

            // 0.16 0.788 
            hand = dd.GetIndexes("7d 8d 2c 2d 6h");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.16 0.68
            hand = dd.GetIndexes("7d 8d 2s Qd Qs");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.16 0.56
            hand = dd.GetIndexes("7h 8h Qd Ah Ad");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));

            //------------------------------------------------------------

            // 0.11 0.71
            hand = dd.GetIndexes("7c 8c 2s Kd 6h");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.11 0.59
            hand = dd.GetIndexes("7c 8c Ad 4h 4s");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));
            
            // 0.11 0.46
            hand = dd.GetIndexes("7c 8c Qd Th As");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));

            //------------------------------------------------------------

            // 0.50 0.88
            hand = dd.GetIndexes("7s 8s 6h As Ts");
            Assert.AreEqual(5, ca.GetAbstractCard(hand, hand.Length));

            // 0.50 0.38
            hand = dd.GetIndexes("7d 8d Jh 8h Ts");
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length));

            //------------------------------------------------------------

            // 0.577 0.859
            hand = dd.GetIndexes("7h 8h 9h 6h Kc");
            Assert.AreEqual(5, ca.GetAbstractCard(hand, hand.Length));

            // 0.577187135837646  0.401744224876062 
            hand = dd.GetIndexes("7c 8c Jd 8h Kc");
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length));

            // 0.582738543555089 0.344213514625024 
            hand = dd.GetIndexes("7d 8d As 5h 7s");
            Assert.AreEqual(7, ca.GetAbstractCard(hand, hand.Length));

            //------------------------------------------------------------

            #endregion

            #region Preflop bucket 5 (87s), flop bucket 5

            // 0.128810521694117 0.951043885127019 
            hand = dd.GetIndexes("7s 8s 6s 5d 5s 3c");
            Assert.AreEqual(3, ca.GetAbstractCard(hand, hand.Length));

            // 0.123812857602944 0.945352984446005 
            hand = dd.GetIndexes("7d 8d 5d 6d 6h 3h");
            Assert.AreEqual(3, ca.GetAbstractCard(hand, hand.Length));

            // 0.12485890128187 0.867666738688815
            hand = dd.GetIndexes("7d 8d 9d 5d 2s 9h");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));

            // 0.12416155385214 0.778243250977976
            hand = dd.GetIndexes("7d 8d 9c 3d Jd Jc");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            #endregion

            #region Preflop bucket 5 (87s), flop bucket 6

            // 0.472642274513544 0.88830932502117
            hand = dd.GetIndexes("7d 8d 7s Td Jh Qd");
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length));

            // 0.470875730450598 0.598779490375814
            hand = dd.GetIndexes("7s 8s Qh 8c Kd Qd");
            Assert.AreEqual(4, ca.GetAbstractCard(hand, hand.Length));

            // 0.473583403574426 0.456663796438368
            hand = dd.GetIndexes("7c 8c Kd Ts 8s 2c");
            Assert.AreEqual(5, ca.GetAbstractCard(hand, hand.Length));

            #endregion

            #region Preflop bucket 5 (87s), flop bucket 5, turn bucket 3
            // 0 0
            hand = dd.GetIndexes("7d 8d 5d 6d 2c 6c 3c");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));

             
            // 0.0370182536334441 0 
            hand = dd.GetIndexes("7c 8c 4c 6s 6c 9s 3h");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));

            // 0.0491886423862399 0
            hand = dd.GetIndexes("7c 8c 6c 5s 2c 5d Qd");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));

            // 0.103955376812209 0
            hand = dd.GetIndexes("7c 8c 4c 6s 6c 9d As");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));

            // 0.122210952460597 0 
            hand = dd.GetIndexes("7c 8c 4c 9c Jh 5d Qh");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));

            // 0.615618648734274 0
            hand = dd.GetIndexes("7c 8c 6c 9c 4s Kd 7d");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.806288057566463 0
            hand = dd.GetIndexes("7h 8h 4h Th 6s Tc 8c");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.946247475857973 0
            hand = dd.GetIndexes("7h 8h 4h Th 6s 3d 6h");
            Assert.AreEqual(3, ca.GetAbstractCard(hand, hand.Length));

            // 1 0
            hand = dd.GetIndexes("7d 8d 5d 6d 2c Jc 4d");
            Assert.AreEqual(3, ca.GetAbstractCard(hand, hand.Length));

            #endregion

        }


        /// <summary>
        /// Test unnormalized values and adaptivity. The latter must be switch on, otherwise k-means 
        /// has trouble with the clustering of values that are very close to each other, like quads on the river.
        /// </summary>
        [Test]
        public void Test_SD3_UnnormalizedAdaptive()
        {
            Props parameters = XmlSerializerExt.Deserialize<Props>(Path.Combine(_testResDir, "ca-hssd-km.xml"));

            parameters.Set("SdKind", "Sd3");
            parameters.Set("NormalizeHandValues", "false");
            parameters.Set("MinBucketCounts", "8 10 10 1");
            parameters.Set("MaxBucketCounts", "8 10 10 4");
            parameters.Set("ClusterSizes0", "0 0.05 0.05 0.05");
            parameters.Set("ClusterSizes1", "0 2 2 0");


            HsSdKMeansAdaptiveCa ca = CalculateCa(parameters, new int[] {0, 5000, 5000, 200}, 1);
            VerifyPreflopPockets(ca);

            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;

            // In comments there are normalized values.

            #region Preflop bucket 3 (87s)

            //------------------------------------------------------------

            // 0.572431087493896 0.380353838205338 
            hand = dd.GetIndexes("7d 8d 5d 5h 4d");
            Assert.AreEqual(5, ca.GetAbstractCard(hand, hand.Length));

            // 0.57322484254837 0.173631072044373
            hand = dd.GetIndexes("7c 8c 7s 2s As");
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length));

            //------------------------------------------------------------
            #endregion

            #region Buckets 4,9,9 (adapted to one cluster, make sure min and max hs go to the same bucket 0)

            // Min hs
            // 0.953535377979279 0
            hand = dd.GetIndexes("5h 5s 3s 5c 3d 2c 3c");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));


            // Max hs
            //  1 0 
            hand = dd.GetIndexes("5h 5s 3d 5d 5c Jc 2h");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));

            //------------------------------------------------------------
            #endregion
        }

        [Test]
        public void Test_SDPlus1_Normalized()
        {
            Props parameters = XmlSerializerExt.Deserialize<Props>(Path.Combine(_testResDir, "ca-hssd-km.xml"));

            parameters.Set("SdKind", "SdPlus1");
            parameters.Set("NormalizeHandValues", "true");

            HsSdKMeansAdaptiveCa ca = CalculateCa(parameters, new int[] {0, 5000, 5000, 200}, 1);
            VerifyPreflopPockets(ca);

            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;

            // In comments there are normalized values.

            #region Preflop bucket 3 (87s)

            //------------------------------------------------------------

            // 0.124870985344431 0.63508321643425 
            hand = dd.GetIndexes("7h 8h 3h 3d 5d");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));


            // 0.125807666466134 0.516141338712588 
            hand = dd.GetIndexes("7d 8d Jh 5s Kc");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.124111506282763 0.417079457241485
            hand = dd.GetIndexes("7c 8c Qd 9h As");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));

            //------------------------------------------------------------
            #endregion

            #region Preflop bucket 5 (87s), flop bucket 2

            // 0.135108591362915 0.296108827079204
            hand = dd.GetIndexes("7c 8c As Jh 5h Ah");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.135741421452917 0.202453184066334 
            hand = dd.GetIndexes("7c 8c Qs 3c As Td");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));


            //  0.371952468197259 0.700541397123714
            hand = dd.GetIndexes("7h 8h Ah Ad 9c Td");
            Assert.AreEqual(6, ca.GetAbstractCard(hand, hand.Length));

            // 0.368155487657242 0.848327075079798
            hand = dd.GetIndexes("7s 8s Kd 6c 6s 2s");
            Assert.AreEqual(5, ca.GetAbstractCard(hand, hand.Length));

            #endregion

            #region Preflop bucket 5 (87s), flop bucket 2, turn bucket 5

            // 0.617032410239351 0
            hand = dd.GetIndexes("7c 8c Kd As 3c 4c 7d");
            Assert.AreEqual(2, ca.GetAbstractCard(hand, hand.Length));

            // 0.0250783709388574 0
            hand = dd.GetIndexes("7c 8c Ah Kd 5c 2c 6h");
            Assert.AreEqual(0, ca.GetAbstractCard(hand, hand.Length));


            // 0.100313477974807 0
            hand = dd.GetIndexes("7c 8c 4s Ah Ts 6d Ad");
            Assert.AreEqual(1, ca.GetAbstractCard(hand, hand.Length));

            #endregion

        }


        #endregion


        #region Benchmarks
        #endregion

        #region Implementation

        string _testResDir = UTHelper.GetTestResourceDir(Assembly.GetExecutingAssembly());


        void VerifyPreflopPockets(HsSdKMeansAdaptiveCa ca)
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


        HsSdKMeansAdaptiveCa CalculateCa(Props parameters, int[] samplesCount, int rngSeed)
        {
            ClusterTree rt = new ClusterTree();
            parameters.Set("IsCreatingClusterTree", "true");
            HsSdKMeansAdaptiveCa ca = new HsSdKMeansAdaptiveCa(parameters);

            CaMcGen gen = new CaMcGen
            {
                Clusterizer = ca,
                IsVerbose = false,
                // IsVerboseSamples = true,
                RngSeed = rngSeed,
                SamplesCount = samplesCount
            };
            rt.Root = gen.Generate();
            string fileName = Path.Combine(_outDir, "ca-hssd-km.dat");
            rt.Write(fileName);

            parameters.Set("ClusterTreeFile", fileName);
            parameters.Set("IsCreatingClusterTree", "false");
            HsSdKMeansAdaptiveCa ca1 = new HsSdKMeansAdaptiveCa(parameters);
            return ca1;
        }

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "HsSdKMeansAdaptiveCa_Test");


        #endregion
    }
}
