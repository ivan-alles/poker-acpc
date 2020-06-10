/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ai.lib.algorithms;
using ai.lib.algorithms.tree;
using ai.lib.kmeans;
using ai.lib.utils;
using ai.pkr.holdem.strategy.core;
using ai.pkr.metagame;
using ai.pkr.metastrategy;
using System.Collections.Generic;
using ai.lib.algorithms.la;

namespace ai.pkr.holdem.strategy.ca
{
    public unsafe abstract class KMeansAdaptiveCaBase : IChanceAbstraction, IClusterizer
    {
        #region Public API
        ///<summary>
        ///<para>Parameters:</para>
        ///<para>IsCreatingClusterTree: (bool, optional, default: false). Indicates that the object is created to create a cluster tree.</para>
        ///<para>ClusterTreeFile: (string, required). File with the cluster tree. Is ignored during cluster tree creation.</para>
        ///<para>MinBucketCounts: (string, required) minimal number of buckets for each round, one-space separated.</para>
        ///<para>MaxBucketCounts: (string, required) maximal number of buckets for each round, one-space separated.</para>
        ///<para>ClusterSizes#: (string, optional, default: 1e-9 1e-9 1e-9 1e-9) for each dimension: space separated lengths of one cluster for each round.
        ///Value for preflop is ignored. For each round the min. number of clusters of all dimesions will be taken.
        ///Zero-cluster sizes are exclued from the calculatíon.</para>
        ///<para>KMeansStages: (int, required)  number of k-means stages.</para>
        ///<para>Pockets#: (string, required) preflop pockets for bucket #.</para>
        ///<para>NormalizeHandValues: (bool, optional, default: false)  normalizes values for each coordinate so that they are in  [0..1].</para>
        ///<para>PrintHands: (bool, optional, default: false)  print sampled hands.</para>
        ///<para>PrintHandValues: (bool, optional, default: false)  for cluster tree creation: if true, prints values for each hand.</para>
        ///</summary>
        public KMeansAdaptiveCaBase(int dim, Props parameters)
        {
            Dim = dim;
            Parameters = parameters;

            _deck = XmlSerializerExt.Deserialize<DeckDescriptor>(
                Props.Global.Expand("${bds.DataDir}ai.pkr.metagame/${0}", "stddeck.xml"));

            bool isCreatingClusterTree = bool.Parse(Parameters.GetDefault("IsCreatingClusterTree", "false"));

            _printHandValues = bool.Parse(Parameters.GetDefault("PrintHandValues", "false"));
            _printHands = bool.Parse(Parameters.GetDefault("PrintHands", "false"));
            _normalizeHandValues = bool.Parse(Parameters.GetDefault("NormalizeHandValues", "false"));


            MaxBucketCounts = ParseBucketsString(Parameters.Get("MaxBucketCounts"));
            MinBucketCounts = ParseBucketsString(Parameters.Get("MinBucketCounts"));

            if (MinBucketCounts[0] != MaxBucketCounts[0])
            {
                throw new ApplicationException(string.Format("Max and min preflop bucket counts must be equal, was: {0}, {1}",
                                                             MinBucketCounts[0], MaxBucketCounts[0]));
            }

            ClusterSizes = new double[Dim][];
            for (int d = 0; d < Dim; ++d)
            {
                ClusterSizes[d] = ParseDoubles(Parameters.GetDefault("ClusterSizes" + d.ToString(), "1e-9 1e-9 1e-9 1e-9"));
            }

            _pfPocketCa = new PreflopPocketCA(parameters, MaxBucketCounts[0]);

            if (_pfPocketCa.PocketKindToAbstrCard == null)
            {
                throw new ApplicationException("Preflop pockets must be specified manually");
            }

            _kmParameters.SetDefaultTerm();
            _kmParameters.dim = Dim;
            _kmParameters.term_st_a = int.Parse(Parameters.Get("KMeansStages"));
            _kmParameters.term_st_b = _kmParameters.term_st_c = _kmParameters.term_st_d = 0;
            _kmParameters.seed = 1;


            if (!isCreatingClusterTree)
            {
                string clusterTreeFile = Parameters.Get("ClusterTreeFile");
                if (!File.Exists(clusterTreeFile))
                {
                    throw new ApplicationException(string.Format("Cluster tree file '{0}' does not exist", clusterTreeFile));
                }
                _clusterTree = ClusterTree.Read(clusterTreeFile);
            }
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

        /// <summary>
        /// For each dimension, for each round: wished size of the cluster.
        /// </summary>
        public double[][] ClusterSizes
        {
            protected set;
            get;
        }

        public bool IsVerbose
        {
            get;
            set;
        }

        public int Dim
        {
            private set;
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
            KMeansNode kn = (KMeansNode)_clusterTree.Root.GetChild(abstrCard);
            double[] point = new double[Dim];
            for (int r = 1; r <= round; ++r)
            {
                CalculateValue(hand, HeHelper.RoundToHandSize[r], point);
                abstrCard = kn.FindClosestChild(point, _normalizeHandValues);
                kn = kn.Children[abstrCard];
            }
            Debug.Assert(abstrCard >= 0 && abstrCard < MaxBucketCounts[round]);
            return abstrCard;
        }

        public Bucket[] BucketizeHands(int round, McHand[] hands, IClusterNode parentNode)
        {
            KMeansNode parentKmNode = (KMeansNode)parentNode;

            if (hands == null || hands.Length == 0)
            {
                parentKmNode.AllocateChildren(1);
                parentKmNode.Children[0] = new KMeansNode(Dim, 0);
                return new Bucket[] { new Bucket() };
            }

            double[][] centers;
            double[][] values = CalcValuesAndKMeans(parentKmNode, round, hands, out centers);

            if (IsVerbose)
            {
                Console.Write("Centers:");
                PrintCenters(centers);
                Console.WriteLine();
            }
            Array.Sort(centers, new CenterComparer());
            if (IsVerbose)
            {
                Console.Write("Sorted centers:");
                PrintCenters(centers);
                Console.WriteLine();
            }

            parentKmNode.AllocateChildren(centers.Length);
            for (int c = 0; c < centers.Length; ++c)
            {
                KMeansNode node = new KMeansNode(Dim, 0);
                centers[c].CopyTo(node.Center, 0);
                parentKmNode.Children[c] = node;
            }

            Bucket[] buckets = new Bucket[parentKmNode.Children.Length].Fill(i => new Bucket());
            for (int i = 0; i < hands.Length; ++i)
            {
                // Never normalize values here, because it is either not necessary or already done (for k-means).
                int abstrCard = parentKmNode.FindClosestChild(values[i], false);
                buckets[abstrCard].Hands.Add(hands[i]);
            }
            return buckets;
        }

        private void PrintCenters(double[][] centers)
        {
            for (int c = 0; c < centers.Length; ++c)
            {
                for (int d = 0; d < Dim; ++d)
                {
                    Console.Write(" " + centers[c][d].ToString("0.000", CultureInfo.InvariantCulture));
                }
                Console.Write("     ");
            }
        }

        public virtual void OnGenerateEnd(IClusterNode root)
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

        public virtual IClusterNode OnGenerateBegin()
        {
            Kml.Init(null);

            KMeansNode root = new KMeansNode(Dim, MaxBucketCounts[0]);
            for (int i = 0; i < root.ChildrenCount; ++i)
            {
                root.Children[i] = new KMeansNode(Dim, 0);
            }
            return root;
        }

        #endregion

        #region Protected 

        /// <summary>
        /// To sort centers ascending by coord. 0, then by coord. 1 etc.
        /// This is usually what we want - the lowest value goes to bucket 0, the highest - to bucket N.
        /// </summary>
        protected class CenterComparer : IComparer<double[]>
        {

            #region IComparer<double[]> Members

            public int Compare(double[] x, double[] y)
            {
                if (x.Length != y.Length)
                {
                    throw new ArgumentException("Array length mismatch");
                }
                for (int i = 0; i < x.Length; ++i)
                {
                    if (x[i] < y[i])
                    {
                        return -1;
                    }
                    if (x[i] > y[i])
                    {
                        return 1;
                    }
                }
                return 0;
            }

            #endregion
        }

        protected int GetPreflopAbstrCard(int[] hand)
        {
            HePocketKind pk = HePocket.HandToKind(hand);
            return _pfPocketCa.PocketKindToAbstrCard[(int)pk];
        }

        protected void PrintVector(double[] v)
        {
            for(int i = 0; i < v.Length; ++i)
            {
                Console.Write("{0} ", v[i].ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Implement to calcualate the value of the hand.
        /// </summary>
        /// <param name="hand">The hand.</param>
        /// <param name="value">Put the value here.</param>
        protected abstract void CalculateValue(int [] hand, int handLength, double[] value);

        /// <summary>
        /// Run k-means. 
        /// Returns hand values.
        /// </summary>
        protected double[][] CalcValuesAndKMeans(KMeansNode parentKmNode, int round, McHand[] hands, out double[][] centers)
        {
            double [][] values = new double[hands.Length][].Fill(i => new double[Dim]);

            double[] min = new double[Dim].Fill(double.MaxValue);
            double[] max = new double[Dim].Fill(double.MinValue);

            for (int i = 0; i < hands.Length; ++i)
            {
                CalculateValue(hands[i].Cards, hands[i].Length, values[i]);
                VectorS.UpdateMin(values[i], min);
                VectorS.UpdateMax(values[i], max);
            }
            double [] delta = VectorS.Sub(max, min);

            if(_normalizeHandValues)
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    VectorS.NormalizeByDiff(values[i], min, delta);
                }
                min.CopyTo(parentKmNode.ValueMin, 0);
                delta.CopyTo(parentKmNode.ValueBounds, 0);
            }

            if (_printHandValues || _printHands)
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    if (_printHands)
                    {
                        Console.Write("{0} ", StdDeck.Descriptor.GetCardNames(hands[i].Cards, 0, hands[i].Length));
                    }
                    if (_printHandValues)
                    {
                        PrintVector(values[i]);
                    }
                    Console.WriteLine();
                }
            }

            // Calcualate number of clusters.
            int k = MaxBucketCounts[round];
            int adaptingDim = -1;
            for (int d = 0; d < Dim; ++d)
            {
                double clusterSize = ClusterSizes[d][round];
                if (clusterSize != 0)
                {
                    int clCount = (int)Math.Round(delta[d] / clusterSize, 0);
                    if (clCount < k)
                    {
                        adaptingDim = d;
                        k = clCount;
                    }
                }
            }
            // Make sure number of clusters is in the given range.
            k = Math.Max(MinBucketCounts[round], k);

            if (IsVerbose)
            {
                Console.Write("Min: ");
                PrintVector(min);
                Console.Write(" Max: ");
                PrintVector(max);
                Console.Write(" Delta: ");
                PrintVector(delta);
                if (k < MaxBucketCounts[round])
                {
                    Console.Write(" K adapted to {0} by dim: {1}", k, adaptingDim);
                }
                Console.WriteLine();
            }

            double [][] differentValues = new double[k][].Fill(i => new double[Dim]);
            int differentValuesCount = 0;
            for (int i = 0; i < values.Length; ++i)
            {
                for (int j = 0; j < differentValuesCount; ++j)
                {
                    if(VectorS.AreEqual(values[i], differentValues[j]))
                    {
                        goto RepeatedValue;
                    }
                }
                values[i].CopyTo(differentValues[differentValuesCount], 0);
                differentValuesCount++;
                if (differentValuesCount == k)
                {
                    break;
                }
            RepeatedValue: ;
            }
            if (differentValuesCount < k)
            {
                // Too few different values to build k clusters. Do not run k-means, it may hang.
                centers = differentValues;
                Array.Resize(ref centers, differentValuesCount);
                if (IsVerbose)
                {
                    Console.WriteLine("Only {0} different values found. Set cluster count, do not run kmeans", differentValuesCount);
                }
                return values;
            }

            _kmParameters.k = k;
            _kmParameters.n = values.Length;

            _kmParameters.Allocate();

            for (int i = 0; i < values.Length; ++i)
            {
                for (int d = 0; d < Dim; ++d)
                {
                    *_kmParameters.GetPoint(i, d) = values[i][d];
                }
            }

            fixed (Kml.Parameters* kmlp = &_kmParameters)
            {
                Kml.KML_Hybrid(kmlp);
            }
            centers = new double[_kmParameters.k][].Fill(i => new double[Dim]);
            for (int c = 0; c < _kmParameters.k; ++c)
            {
                for (int d = 0; d < Dim; ++d)
                {
                    centers[c][d] = *_kmParameters.GetCenter(c, d);
                }
            }
            _kmParameters.Free();

            return values;
        }

        protected int CalculateEstimatedNodeCount(int[] bucketsCount)
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

        protected static double[] ParseDoubles(string text)
        {
            string[] strings = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double[] values = (new double[strings.Length]).Fill(i => double.Parse(strings[i], CultureInfo.InvariantCulture));

            if (values.Length != 4)
            {
                throw new ArgumentException(string.Format("Number of elements must be 4, was {0}", values.Length));
            }
            return values;
        }

        protected static int[] ParseBucketsString(string bucketsString)
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
        protected DeckDescriptor _deck;

        protected PreflopPocketCA _pfPocketCa;
        protected ClusterTree _clusterTree;
        protected bool _printHands;
        protected bool _printHandValues;
        protected bool _normalizeHandValues;
        protected Kml.Parameters _kmParameters;

        #endregion
    }
}