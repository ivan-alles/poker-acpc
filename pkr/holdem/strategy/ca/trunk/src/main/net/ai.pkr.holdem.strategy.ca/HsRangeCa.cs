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

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A chance abstraction for HE based on HS. It divides HS of all hands in each node of the cluster tree
    /// into ranges from min to max and bucketizes the hands based on this range and number of buckets.
    /// Preflop pockets must be selected manually. 
    /// </summary>
    public class HsRangeCa : IChanceAbstraction, IClusterizer
    {

        #region Public API
        ///<summary>
        ///<para>Parameters:</para>
        ///<para>IsCreatingClusterTree: (bool, optional, default: false). Indicates that the object is created to create a cluster tree.</para>
        ///<para>ClusterTreeFile: (string, required). File with the cluster tree. Is ignored during cluster tree creation.</para>
        ///<para>BucketCounts: (string, required) number of buckets for each round, one-space separated.</para>
        ///<para>Pockets#: (string, required) preflop pockets for bucket #.</para>
        ///</summary>
        public HsRangeCa(Props parameters)
        {
            Parameters = parameters;

            _deck = XmlSerializerExt.Deserialize<DeckDescriptor>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metagame/${0}", "stddeck.xml"));

            bool isCreatingClusterTree = false;
            if(!string.IsNullOrEmpty(Parameters.Get("IsCreatingClusterTree")))
            {
                isCreatingClusterTree = bool.Parse(Parameters.Get("IsCreatingClusterTree"));
            }


            string bucketCountsString = Parameters.Get("BucketCounts");
            string[] bucketCountsText = bucketCountsString.Split(new char[] { ' ' },
                                                                             StringSplitOptions.RemoveEmptyEntries);
            BucketCounts = (new int[bucketCountsText.Length]).Fill(i => int.Parse(bucketCountsText[i]));

            if (BucketCounts.Length != 4)
            {
                throw new ArgumentException(string.Format("Bucket counts must be 4, was {0}", BucketCounts.Length));
            }


            _pfPocketCa = new PreflopPocketCA(parameters, BucketCounts[0]);

            if (_pfPocketCa.PocketKindToAbstrCard == null)
            {
                throw new ApplicationException("Preflop pockets must be specified manually");
            }

            if(!isCreatingClusterTree)
            {
                string clusterTreeFile = Parameters.Get("ClusterTreeFile");
                if(!File.Exists(clusterTreeFile))
                {
                    throw new ApplicationException(string.Format("Cluster tree file '{0}' does not exist", clusterTreeFile));
                }
                _clusterTree = ClusterTree.Read(clusterTreeFile);
            }

            string shortDescription = Parameters.Get("ShortDescription") ?? "";

            Name = "HE-HSR-" + shortDescription + "-" + bucketCountsString.Replace(" ", "x");
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

        public int[] BucketCounts
        {
            protected set;
            get;
        }

        public int GetAbstractCard(int[] hand, int handLength)
        {
            int round = HeHelper.HandSizeToRound[handLength];
            int abstrCard = GetPreflopAbstrCard(hand);
            if(round == 0)
            {
                return abstrCard;
            }
            RangeNode rn = (RangeNode)_clusterTree.Root.GetChild(abstrCard);
            for(int r = 1; r <= round; ++r)
            {
                float hs = HandStrength.CalculateFast(hand, HeHelper.RoundToHandSize[r]);
                abstrCard = rn.FindChildByValue(hs);
                rn = rn.Children[abstrCard];
            }
            Debug.Assert(abstrCard >= 0 && abstrCard < BucketCounts[round]);
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
            RangeNode root = new RangeNode(BucketCounts[0]);
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
                RangeNode node = new RangeNode {UpperLimit = ranges[i]};
                parentRangeNode.Children[i] = node;
            }

            if (hands == null || hands.Length == 0)
            {
                return new Bucket[]{new Bucket()};
            }

            Bucket[] buckets = new Bucket[parentRangeNode.Children.Length].Fill(i => new Bucket());
            for(int i = 0; i < hands.Length;++i)
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
            for(int i = 0; i < hands.Length; ++i)
            {
                float value = HandStrength.CalculateFast(hands[i].Cards, hands[i].Length);
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
            int rangesCount = BucketCounts[round];
            float[] ranges = new float[rangesCount];
            float step = (max - min) / rangesCount;
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
                int estimatedNodeCount = 1;
                int power = 1;
                for (int r = 0; r < 4; ++r)
                {
                    power *= BucketCounts[r];
                    estimatedNodeCount += power;
                }
                int actualNodeCount = CountNodes<int>.Count<ClusterTree, IClusterNode>(null, root);
                Console.WriteLine("Nodes: estimated: {0:#,#}, actual: {1:#,#}",
                    estimatedNodeCount, actualNodeCount);
            }
        }

        /// <summary>
        /// Deck descriptor. Create an own instance to be thread-safe.
        /// </summary>
        DeckDescriptor _deck;

        PreflopPocketCA _pfPocketCa;

        ClusterTree _clusterTree;

        #endregion
    }

}
