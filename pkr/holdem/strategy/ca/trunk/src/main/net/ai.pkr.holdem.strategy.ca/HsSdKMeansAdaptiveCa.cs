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

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A chance abstraction for HE based on HS SD. Preflop pockets must be selected manually. 
    /// </summary>
    public unsafe class HsSdKMeansAdaptiveCa : KMeansAdaptiveCaBase
    {

        #region Public API
        ///<summary>
        ///<para>Parameters:</para>
        ///<para>SdKind: (HsSd.SdKind, required). SD kind (Sd3 or SdPlus1) for round 1. 
        ///For round 2 they are equal, for 3 - Sd3 is always used (sd == 0).</para>
        ///</summary>
        public HsSdKMeansAdaptiveCa(Props parameters)
            : base(2, parameters)
        {
            string sdKind = Parameters.Get("SdKind");
            if (sdKind == "Sd3")
            {
                _sdKind = HsSd.SdKind.Sd3;
            }
            else if (sdKind == "SdPlus1")
            {
                _sdKind = HsSd.SdKind.SdPlus1;
            }
            else
            {
                throw new ApplicationException(String.Format("Unknown SD kind '{0}'", sdKind));
            }
            Name = "HE-HSSDKMA-" + Parameters.GetDefault("ShortDescription", "") + "-" +
                Parameters.Get("MaxBucketCounts").Replace(" ", "x");
        }

        #endregion

        #region Implementation

        public override IClusterNode OnGenerateBegin()
        {
            if (IsVerbose)
            {
                Console.WriteLine("SD Kind: {0}, norm hand values: {1}", _sdKind, _normalizeHandValues);
            }
            return base.OnGenerateBegin();
        }

        protected override void CalculateValue(int[] hand, int handLength, double[] value)
        {
            float[] hssd = HsSd.CalculateFast(hand, handLength, handLength == 7 ? HsSd.SdKind.Sd3 : _sdKind);
            value[0] = hssd[0];
            value[1] = hssd[1];
        }

        HsSd.SdKind _sdKind;
        #endregion
    }

}
