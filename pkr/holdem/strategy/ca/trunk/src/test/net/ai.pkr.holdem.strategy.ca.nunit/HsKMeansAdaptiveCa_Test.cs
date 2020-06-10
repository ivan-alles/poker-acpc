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
    /// Unit tests for HsKMeansAdaptiveCa. 
    /// </summary>
    [TestFixture]
    public class HsKMeansAdaptiveCa_Test
    {
        #region Tests

        [Test]
        public void Test_GetBucket_Preview()
        {
            string minBucketsCounts = "8 1 1 1";
            string maxBucketsCounts = "8 7 6 5";
            HsKMeansAdaptiveCa b = CalculateCa(minBucketsCounts, maxBucketsCounts,
                "0.1 0.1 0.1 0.1", new int[] { 0, 1000, 1000, 1000 }, false);

            Console.WriteLine("Bucket counts: {0}", maxBucketsCounts);

            string[] d = new string[]
                            {
                                "Ac Ad",
                                "Ac Kc",
                                "7c 2d",
                                "3c 2d",
                                "Tc 9c",
                                "8c 7c",
                                "Ac Kc Tc Qc Jc",
                                "3c 3d Ac 7d 3h",
                                "7d 3d Ac 3c 3h",
                                "Ad Ah Ac 7d 3h",
                                "Ac Ah Qs 7d 6d",
                                "5d 4d Qs 7d 6d",
                                "8d 9d Qs 7d 6d",
                                "Ad Ah Ac 7d 3h 5h",
                                "Ad Ah Ac 7d 3h 5h 5d",
                                "5c 5s Ac 7d 3h 5h 5d",
                                "3c 2d Kc Td 7h",
                                "3c 2d Kc Td 7h 5h Qs",
                                "Js Jd 7h 8h 9h",
                                "Js Jd 7h 8h 9h Jc",
                                "Js Jd 7h 8h 9h 6h",
                                "8c 7c",
                                "8c 7c Ad Qc Jd",
                                "8c 7c Ad Qc Jd 6c",
                                "8c 7c Ad Qc Jd 6c 5c",
                            };

            for (int i = 0; i < d.Length; ++i)
            {
                int[] hand = StdDeck.Descriptor.GetIndexes(d[i]);
                int bucket = b.GetAbstractCard(hand, hand.Length);
                double hs = HandStrength.CalculateFast(hand);
                Console.WriteLine("Hand '{0}' has strength {1:0.0000}, bucket {2}", d[i], hs, bucket);
            }
        }

        /// <summary>
        /// Verify some obvious situations.
        /// </summary>
        [Test]
        public void Test_GetBucket()
        {
            // Fix number of buckets to get exact abstract cards.
            string minBucketsCounts = "8 7 6 5";
            string maxBucketsCounts = "8 7 6 5";
            HsKMeansAdaptiveCa b = CalculateCa(minBucketsCounts, maxBucketsCounts,
                "0.1 0.1 0.1 0.1", new int[] { 0, 1000, 1000, 1000 }, false);

            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;

            // Preflop
            hand = dd.GetIndexes("Ac Ah");
            Assert.AreEqual(7, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("7c 2d");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("3d 2c");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));

            // Flop
            hand = dd.GetIndexes("Ac Ah As Ad 2h");
            Assert.AreEqual(6, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("2c 3d As Ks Qh");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));

            // Turn
            hand = dd.GetIndexes("Ac Ah As Ad 2h 2d");
            Assert.AreEqual(5, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("6c 7c 3c 4c 5c As");
            Assert.AreEqual(5, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("2c 3d 7s 5s Qh 9d");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));

            // River
            hand = dd.GetIndexes("Ac Ah As Ad 2h 2d Qh");
            Assert.AreEqual(4, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("6c 7c 8d 4c 5c As 8c");
            Assert.AreEqual(4, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("2c 3d As Ks Qh 9d 6h");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));

            // Runner - runner
            hand = dd.GetIndexes("8c 7c");
            Assert.AreEqual(2, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("8c 7c Ad Qc Jd");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("8c 7c Ad Qc Jd 4c");
            int abstrCard = b.GetAbstractCard(hand, hand.Length);
            Assert.IsTrue(0 < abstrCard && abstrCard < 5, "Not worst, not best");
            hand = dd.GetIndexes("8c 7c Ad Qc Jd 4c 5c");
            Assert.AreEqual(4, b.GetAbstractCard(hand, hand.Length));
        }

        /// <summary>
        /// Use preflop pockets.
        /// </summary>
        [Test]
        public void Test_GetBucket_PreflopPocket()
        {
            string minBucketsCounts = "8 1 1 1";
            string maxBucketsCounts = "8 7 6 5";
            HsKMeansAdaptiveCa b = CalculateCa(minBucketsCounts, maxBucketsCounts,
                "0.1 0.1 0.1 0.1", new int[] { 0, 1000, 1000, 1000 }, false);

            DeckDescriptor dd = StdDeck.Descriptor;
            int[] hand;

            hand = dd.GetIndexes("7c 2d");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("6d 5d");
            Assert.AreEqual(1, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("Ac Ah");
            Assert.AreEqual(7, b.GetAbstractCard(hand, hand.Length));
            hand = dd.GetIndexes("3d 2c");
            Assert.AreEqual(0, b.GetAbstractCard(hand, hand.Length));
        }


        #endregion


        #region Benchmarks
        #endregion

        #region Implementation

        HsKMeansAdaptiveCa CalculateCa(string minBucketCounts, string maxBucketCounts, string clusterSizes, int[] samplesCount, bool isVerbose)
        {
            ClusterTree rt = new ClusterTree();
            Props parameters = GetParams(minBucketCounts, maxBucketCounts, clusterSizes);
            parameters.Set("IsCreatingClusterTree", "true");
            HsKMeansAdaptiveCa ca = new HsKMeansAdaptiveCa(parameters);

            CaMcGen gen = new CaMcGen
            {
                Clusterizer = ca,
                IsVerbose = isVerbose,
                // IsVerboseSamples = true,
                RngSeed = 0,
                SamplesCount = samplesCount
            };
            rt.Root = gen.Generate();
            string fileName = Path.Combine(_outDir, maxBucketCounts.Replace(' ', 'x')) + ".dat";
            rt.Write(fileName);

            parameters.Set("ClusterTreeFile", fileName);
            parameters.Set("IsCreatingClusterTree", "false");
            HsKMeansAdaptiveCa ca1 = new HsKMeansAdaptiveCa(parameters);
            return ca1;
        }

        private Props GetParams(string minBucketCounts, string maxBucketCounts, string clusterSizes)
        {
            Props p = new string[]
                          {
                            "TypeName", typeof(HsKMeansAdaptiveCa).AssemblyQualifiedName,
                            "AssemblyFileName", "",
                            "MinBucketCounts", minBucketCounts,
                            "MaxBucketCounts", maxBucketCounts,
                            "ClusterSizes", clusterSizes,
                            "KMeansStages", "300",
                            "Pockets7", "AA KK QQ JJ TT 99",
                            "Pockets6", "88 AKs 77 AQs AJs AKo ATs AQo AJo KQs 66 A9s ATo KJs A8s KTs",
                            "Pockets5", "KQo A7s A9o KJo 55 QJs K9s A5s A6s A8o KTo QTs A4s A7o K8s A3s QJo K9o A5o A6o Q9s K7s JTs A2s QTo 44 A4o K6s",
                            "Pockets4", "K8o Q8s A3o K5s J9s Q9o JTo K7o A2o K4s Q7s K6o K3s T9s J8s 33 Q6s Q8o K5o J9o K2s Q5s",
                            "Pockets3", "K4o T8s J7s Q4s Q7o T9o J8o K3o Q6o Q3s 98s T7s J6s K2o 22 Q2s Q5o J5s T8o J7o Q4o 97s J4s T6s",
                            "Pockets2", "J3s Q3o 98o 87s T7o J6o 96s J2s Q2o T5s J5o T4s 97o 86s J4o T6o 95s T3s 76s J3o 87o T2s 85s 96o J2o T5o 94s 75s T4o",
                            "Pockets1", "93s 86o 65s 84s 95o T3o 92s 76o 74s T2o 54s 85o 64s 83s 94o 75o 82s 73s 93o 65o 53s 63s 84o 92o 43s 74o",
                          };
            return p;
        }

        string _outDir = UTHelper.MakeAndGetTestOutputDir(Assembly.GetExecutingAssembly(), "HsRangeCa_Test");


        #endregion
    }
}
