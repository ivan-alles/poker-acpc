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
using ai.lib.kmeans;

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A chance abstraction for HE based on HS. It divides HS of all hands in each node of the cluster tree
    /// into ranges from min to max. It decides that how many buckets it will created based on parameters 
    /// and bucketizes the hands based on this range and number of buckets.
    /// Preflop pockets must be selected manually. 
    /// </summary>
    public unsafe class HsKMeansAdaptiveCa : KMeansAdaptiveCaBase
    {

        #region Public API
        ///<summary>
        ///</summary>
        public HsKMeansAdaptiveCa(Props parameters) : base(1, parameters)
        {
            Name = "HE-HSKMA-" + Parameters.GetDefault("ShortDescription", "") + "-" + 
                Parameters.Get("MaxBucketCounts").Replace(" ", "x");
        }

        #endregion

        #region Implementation
        protected override void CalculateValue(int[] hand, int handLength, double[] value)
        {
            value[0] = HandStrength.CalculateFast(hand, handLength);
        }

        #endregion
    }

}
