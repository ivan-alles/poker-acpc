/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.lib.utils;
using ai.pkr.metagame;
using ai.pkr.holdem.strategy.core;
using ai.lib.algorithms;
using ai.pkr.holdem.strategy.hs;
using System.Diagnostics;
using ai.lib.algorithms.tree;
using System.IO;
using System.Globalization;

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A chance abstraction for HE based on HS. It divides HS of all hands in each node of the cluster tree
    /// into ranges from min to max. It decides that how many buckets it will created based on parameters 
    /// and bucketizes the hands based on this range and number of buckets.
    /// Preflop pockets must be selected manually. 
    /// </summary>
    public class HsRangeAdaptiveCa : IChanceAbstraction, IClusterizer
    {

        #region Public API
        ///<summary>
        ///<para>Parameters:</para>
        ///<para>IsCreatingClusterTree: (bool, optional, default: false). Indicates that the object is created to create a cluster tree.</para>
        ///<para>ClusterTreeFile: (string, required). File with the cluster tree. Is ignored during cluster tree creation.</para>
        ///<para>MinBucketCounts: (string, required) minimal number of buckets for each round, one-space separated.</para>
        ///<para>MaxBucketCounts: (string, required) maximal number of buckets for each round, one-space separated.</para>
        ///<para>ClusterSizes: (string, required)  for each round: space separated HS range for one cluster. Value for preflop is ignored.</para>
        ///<para>Pockets#: (string, required) preflop pockets for bucket #.</para>
        ///<para>PrintHandValues: (bool, optional)  for cluster tree creation: if true, prints values for each hand.</para>
        ///</summary>
        public HsRangeAdaptiveCa(Props parameters)
        {
            Parameters = parameters;

            _deck = XmlSerializerExt.Deserialize<DeckDescriptor>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metagame/${0}", "stddeck.xml"));

            bool isCreatingClusterTree = false;
            if (!string.IsNullOrEmpty(Parameters.Get("IsCreatingClusterTree")))
            {
                isCreatingClusterTree = bool.Parse(Parameters.Get("IsCreatingClusterTree"));
            }

            _printHandValues = false;
            if (!string.IsNullOrEmpty(Parameters.Get("PrintHandValues")))
            {
                _printHandValues = bool.Parse(Parameters.Get("PrintHandValues"));
            }


            MaxBucketCounts = ParseBucketsString(Parameters.Get("MaxBucketCounts"));
            MinBucketCounts = ParseBucketsString(Parameters.Get("MinBucketCounts"));

            if (MinBucketCounts[0] != MaxBucketCounts[0])
            {
                throw new ApplicationException(string.Format("Max and min preflop bucket counts must be equal, was: {0}, {1}", 
                    MinBucketCounts[0], MaxBucketCounts[0]));
            }

            ClusterSizes = ParseDoubles(Parameters.Get("ClusterSizes"));

            _pfPocketCa = new PreflopPocketCA(parameters, MaxBucketCounts[0]);

            if (_pfPocketCa.PocketKindToAbstrCard == null)
            {
                throw new ApplicationException("Preflop pockets must be specified manually");
            }

            if (!isCreatingClusterTree)
            {
                string clusterTreeFile = Parameters.Get("ClusterTreeFile");
                if (!File.Exists(clusterTreeFile))
                {
                    throw new ApplicationException(string.Format("Cluster tree file '{0}' does not exist", clusterTreeFile));
                }
                _clusterTree = ClusterTree.Read(clusterTreeFile);
            }

            string shortDescription = Parameters.Get("ShortDescription") ?? "";

            Name = "HE-HSRA-" + shortDescription + "-" + Parameters.Get("MaxBucketCounts").Replace(" ", "x");
        }

        public string Name
        {
            get;
            protected set;
        }

        public Props Parameters
        {
            set;
            get;
        }


        public int[] MinBucketCounts
        {
            protected set;
            get;
        }

        public int[] MaxBucketCounts
        {
            protected set;
            get;
        }

        public double[] ClusterSizes
        {
            protected set;
            get;
        }

        public int GetAbstractCard(int[] hand, int handLength)
        {
            int round = HeHelper.HandSizeToRound[handLength];
            int abstrCard = GetPreflopAbstrCard(hand);
            if (round == 0)
            {
                return abstrCard;
            }
            RangeNode rn = (RangeNode)_clusterTree.Root.GetChild(abstrCard);
            for (int r = 1; r <= round; ++r)
            {
                float hs = HandStrength.CalculateFast(hand, HeHelper.RoundToHandSize[r]);
                abstrCard = rn.FindChildByValue(hs);
                rn = rn.Children[abstrCard];
            }
            Debug.Assert(abstrCard >= 0 && abstrCard < MaxBucketCounts[round]);
            return abstrCard;
        }

        private int GetPreflopAbstrCard(int[] hand)
        {
            HePocketKind pk = HePocket.HandToKind(hand);
            return _pfPocketCa.PocketKindToAbstrCard[(int)pk];
        }

        #endregion

        #region Implementation

        public bool IsVerbose
        {
            get;
            set;
        }

        public IClusterNode OnGenerateBegin()
        {
            RangeNode root = new RangeNode(MaxBucketCounts[0]);
            for (int i = 0; i < root.ChildrenCount; ++i)
            {
                root.Children[i] = new RangeNode();
            }
            return root;
        }

        public Bucket[] BucketizeHands(int round, McHand[] hands, IClusterNode parentNode)
        {
            RangeNode parentRangeNode = (RangeNode)parentNode;

            float[] ranges;
            float[] values;
            ranges = CalculateRanges(round, hands, out values);
            
            if (IsVerbose)
            {
                Console.Write("Ranges:");
                for (int i = 0; i < ranges.Length; ++i)
                {
                    Console.Write(" {0:0.000}", ranges[i]);
                }
                Console.WriteLine();
            }

            parentRangeNode.AllocateChildren(ranges.Length);
            for (int i = 0; i < ranges.Length; ++i)
            {
                RangeNode node = new RangeNode { UpperLimit = ranges[i] };
                parentRangeNode.Children[i] = node;
            }

            if (hands == null || hands.Length == 0)
            {
                return new Bucket[] { new Bucket() };
            }

            Bucket[] buckets = new Bucket[parentRangeNode.Children.Length].Fill(i => new Bucket());
            for (int i = 0; i < hands.Length; ++i)
            {
                int abstrCard = parentRangeNode.FindChildByValue(values[i]);
                buckets[abstrCard].Hands.Add(hands[i]);
            }
            return buckets;
        }

        /// <summary>
        /// Returns exclusive upper limits for ranges.
        /// </summary>
        float[] CalculateRanges(int round, McHand[] hands, out float[] values)
        {
            if (hands == null || hands.Length == 0)
            {
                if (IsVerbose)
                {
                    Console.WriteLine("Empty parent bucket");
                }
                values = new float[0];
                return new float[] { float.PositiveInfinity };
            }
            values = new float[hands.Length];
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i < hands.Length; ++i)
            {
                float value = HandStrength.CalculateFast(hands[i].Cards, hands[i].Length);
                if (_printHandValues)
                {
                    Console.WriteLine(value);
                }
                values[i] = value;
                if (value < min)
                {
                    min = value;
                }
                if (value > max)
                {
                    max = value;
                }
            }
            float delta = max - min;

            int rangesCount = (int)Math.Round(delta / ClusterSizes[round], 0);

            rangesCount = Math.Max(MinBucketCounts[round], rangesCount);
            rangesCount = Math.Min(MaxBucketCounts[round], rangesCount);

            float[] ranges = new float[rangesCount];
            float step = delta / rangesCount;
            for (int i = 0; i < rangesCount - 1; ++i)
            {
                ranges[i] = (i + 1) * step + min;
            }
            ranges[rangesCount - 1] = float.PositiveInfinity;

            return ranges;
        }


        public void OnGenerateEnd(IClusterNode root)
        {
            if (IsVerbose)
            {
                int estimatedMaxNodeCount = CalculateEstimatedNodeCount(MaxBucketCounts);
                int estimatedMinNodeCount = CalculateEstimatedNodeCount(MinBucketCounts);
                int actualNodeCount = CountNodes<int>.Count<ClusterTree, IClusterNode>(null, root);
                Console.WriteLine("Nodes: estimated min: {0:#,#}, estimated max: {1:#,#}, actual: {2:#,#}",
                    estimatedMinNodeCount, estimatedMaxNodeCount, actualNodeCount);
            }
        }

        private int CalculateEstimatedNodeCount(int [] bucketsCount)
        {
            int estimatedNodeCount = 1;
            int power = 1;
            for (int r = 0; r < 4; ++r)
            {
                power *= bucketsCount[r];
                estimatedNodeCount += power;
            }
            return estimatedNodeCount;
        }

        static double[] ParseDoubles(string text)
        {
            string[] strings = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double[] values = (new double[strings.Length]).Fill(i => double.Parse(strings[i], CultureInfo.InvariantCulture));

            if (values.Length != 4)
            {
                throw new ArgumentException(string.Format("Number of elements must be 4, was {0}", values.Length));
            }
            return values;
        }

        static int[] ParseBucketsString(string bucketsString)
        {
            string[] bucketCountsText = bucketsString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int[] bucketCounts = (new int[bucketCountsText.Length]).Fill(i => int.Parse(bucketCountsText[i]));

            if (bucketCounts.Length != 4)
            {
                throw new ArgumentException(string.Format("Bucket counts must be 4, was {0}", bucketCounts.Length));
            }
            return bucketCounts;
        }

        /// <summary>
        /// Deck descriptor. Create an own instance to be thread-safe.
        /// </summary>
        DeckDescriptor _deck;

        PreflopPocketCA _pfPocketCa;

        ClusterTree _clusterTree;

        bool _printHandValues;
        
        #endregion
    }

}
