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
using ai.pkr.holdem.strategy.hssd;
using ai.pkr.holdem.strategy.ahvo;

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A chance abstraction for HE based on HS SD3 and AHVO. 
    /// Preflop pockets must be selected manually. Values must be normalized.
    /// </summary>
    public unsafe class HsSdAhvoKMeansAdaptiveCa : KMeansAdaptiveCaBase
    {

        #region Public API
        ///<summary>
        ///<para>Parameters:</para>
        ///</summary>
        public HsSdAhvoKMeansAdaptiveCa(Props parameters)
            : base(3, parameters)
        {
            Name = "HE-HsSdAhvoKmA-" + Parameters.GetDefault("ShortDescription", "") + "-" +
                Parameters.Get("MaxBucketCounts").Replace(" ", "x");
            if (!_normalizeHandValues)
            {
                throw new ApplicationException("Normalizing values must be used");
            }
        }

        #endregion

        #region Implementation

        public override IClusterNode OnGenerateBegin()
        {
            if (IsVerbose)
            {
                Console.WriteLine("Norm hand values: {0}", _normalizeHandValues);
            }
            return base.OnGenerateBegin();
        }

        protected override void CalculateValue(int[] hand, int handLength, double[] value)
        {
            float[] hssd = HsSd.CalculateFast(hand, handLength, HsSd.SdKind.Sd3);
            value[0] = hssd[0];
            value[1] = hssd[1];
            value[2] = AHVO.CalculateFast(hand, 2, handLength - 2);
        }

        #endregion
    }

}

