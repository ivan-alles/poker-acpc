/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Linq;
using ai.pkr.metagame;
using ai.pkr.holdem.strategy.core;
using System.Diagnostics;
using ai.lib.algorithms;
using ai.lib.algorithms.random;
using ai.pkr.holdem.strategy.hs;
using ai.lib.algorithms.tree;

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// Monte-carlo CA generator.
    /// </summary>
    public class CaMcGen
    {
        #region Public API
        /// <summary>
        /// Is called periodically to give feedback to the caller.
        /// </summary>
        /// <param name="currentSamplesCount">Current number of samples.</param>
        /// <returns>Return false to stop generation.</returns>
        public delegate bool FeedbackDelegate(Int64 currentSamplesCount);

        public int RngSeed
        {
            set;
            get;
        }

        public FeedbackDelegate Feedback
        {
            set;
            get;
        }

        public bool IsVerbose
        {
            set;
            get;
        }

        public bool IsVerboseSamples
        {
            set;
            get;
        }


        public IClusterizer Clusterizer
        {
            set;
            get;
        }

        /// <summary>
        /// For each round: number of samples per node.
        /// Round 0 is ignored.
        /// </summary>
        public int[] SamplesCount
        {
            set;
            get;
        }

        public IClusterNode Generate()
        {
            _rng = new MersenneTwister(RngSeed);
            _totalMcSamples = 0;

            Clusterizer.IsVerbose = IsVerbose;

            IClusterNode root = Clusterizer.OnGenerateBegin();
            Bucket [] buckets = CreatePreflopBuckets(root.ChildrenCount);
            for (int i = 0; i < root.ChildrenCount; ++i)
            {
                CalculateNode(i.ToString(), 1, root.GetChild(i), buckets[i]);
            }
            if (IsVerbose)
            {
                double [] minClusters = new double[4].Fill(i => double.MaxValue);
                double  [] maxClusters = new double[4].Fill(i => double.MinValue);

                CalculateStatistics(root, 0, minClusters, maxClusters);
                Console.Write("Min clusters count:");
                for (int r = 0; r < minClusters.Length; ++r)
                {
                    Console.Write("{0,3} ", minClusters[r]);
                }
                Console.WriteLine();
                Console.Write("Max clusters count:");
                for (int r = 0; r < maxClusters.Length; ++r)
                {
                    Console.Write("{0,3} ", maxClusters[r]);
                }
                Console.WriteLine();

                Console.WriteLine("Samples: {0:#,#}", _totalMcSamples);
            }
            Clusterizer.OnGenerateEnd(root);
            return root;
        }

        private void CalculateStatistics(IClusterNode n, int round, double [] minClusters, double []maxClusters)
        {
            if(round == 4)
            {
                return;
            }
            int clustersCount = n.ChildrenCount;
            minClusters[round] = Math.Min(minClusters[round], clustersCount);
            maxClusters[round] = Math.Max(maxClusters[round], clustersCount);
            for (int c = 0; c < n.ChildrenCount; ++c)
            {
                CalculateStatistics(n.GetChild(c), round + 1, minClusters, maxClusters);
            }
        }


        #endregion

        #region Implementation

        void CalculateNode(string path, int round, IClusterNode parentNode, Bucket parentBucket)
        {
            if (round == 4)
            {
                return;
            }

            if (IsVerbose)
            {
                Console.WriteLine("Start node: {0}.", path);
            }

            McHand[] hands = null;
            hands = CreateMcHands(round, parentBucket);
            Bucket[] buckets = Clusterizer.BucketizeHands(round, hands, parentNode);

            for (int i = 0; i < parentNode.ChildrenCount; ++i)
            {
                CalculateNode(path+","+i.ToString(),  round + 1, parentNode.GetChild(i), buckets[i]);
            }

            if (IsVerbose)
            {
                Console.WriteLine("Finish node: {0}.", path);
            }
        }

        

        McHand[] CreateMcHands(int round, Bucket parentBucket)
        {
            if (parentBucket.Length == 0)
            {
                if (IsVerbose)
                {
                    Console.WriteLine("Empty parent bucket");
                }
                return new McHand[0];
            }
            int samplesCount = (SamplesCount[round] / parentBucket.Length + 1) * parentBucket.Length;
            McHand[] hands = new McHand[samplesCount];
            int shuffleCount = HeHelper.RoundToHandSize[round] - HeHelper.RoundToHandSize[round - 1];
            int s = 0;
            if (IsVerbose)
            {
                Console.WriteLine("Creating random hands: parent hands: {0}, samples: {1}, deal: {2}",
                    parentBucket.Length, samplesCount, shuffleCount);
            }
            for (int h = 0; h < parentBucket.Length; ++h)
            {
                for (int rep = 0; rep < samplesCount / parentBucket.Length; ++rep)
                {
                    McHand hand = new McHand(parentBucket.Hands[h]);
                    SequenceRng.Shuffle(_rng, hand.Cards, hand.Length, shuffleCount);
                    hand.Length += shuffleCount;
                    hands[s++] = hand;
                    if (IsVerboseSamples)
                    {
                        Console.WriteLine(StdDeck.Descriptor.GetCardNames(hand.Cards, 0, hand.Length));
                    }
                }
            }
            Debug.Assert(s == samplesCount);
            _totalMcSamples += samplesCount;
            return hands;
        }

        

        private Bucket[] CreatePreflopBuckets(int preflopBucketsCount)
        {
            Bucket[] buckets = new Bucket[preflopBucketsCount].Fill(i => new Bucket());

            int totalHands = 0;
            for (int i = 0; i < HePocket.Count; ++i)
            {
                HePocketKind pk = (HePocketKind)i;
                // Use all possible pockets for each pocket kind. This ensures
                // that they occur with the natural frequency in a typical case where 
                // a bucket contain pocket kinds with different numbers of pockets (e.g. AA - 6, AKs - 4, AKo - 12).
                CardSet [] range = HePocket.KindToRange(pk);
                foreach (CardSet pocketCs in range)
                {
                    McHand hand = new McHand();
                    int[] pocket = StdDeck.Descriptor.GetIndexesAscending(pocketCs).ToArray();
                    CardSet restCs = StdDeck.Descriptor.FullDeck;
                    restCs.Remove(pocketCs);
                    int[] rest = StdDeck.Descriptor.GetIndexesAscending(restCs).ToArray();
                    Debug.Assert(pocket.Length + rest.Length == 52);

                    pocket.CopyTo(hand.Cards, 0);
                    rest.CopyTo(hand.Cards, 2);
                    hand.Length = 2;

                    int abstrCard = Clusterizer.GetAbstractCard(hand.Cards, hand.Length);
                    buckets[abstrCard].Hands.Add(hand);
                    totalHands++;
                }
            }
            Debug.Assert(totalHands == 1326);
            if (IsVerbose)
            {
                Console.WriteLine("Preflop buckets created, buckets: {0}, hands: {1}", buckets.Length, totalHands);
            }

            return buckets;
        }

        #endregion

        Random _rng;
        Int64 _totalMcSamples = 0;
    }
}
