/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;

namespace ai.pkr.holdem.strategy.core
{
    /// <summary>
    /// A helper to implement preflop chance abstraction by direct assignment of HE pockets to buckets.
    /// </summary>
    public class PreflopPocketCA
    {
        /// <summary>
        /// Initializes a class from properties.
        /// For each preflop bucket it expects a a property in the following format:
        /// <para>name: "Pockets7", value: "AA KK QQ JJ TT 99 AKs"</para>
        /// <para>If there is no such properties at all, it is assumed that this CA does 
        /// not use preflop pocket bucketizing. In this case the property PocketKindToAbstrCard returns null.</para>
        /// <para>If any such a property exists, than all other in range [1..bucketsCount-1] must be specified, 
        /// otherwise an ArgumentException it thrown.</para>
        /// <para>All pockets unspecified in such properties go to bucket 0 (even if its property specifed explicitely).</para>
        /// </summary>
        public PreflopPocketCA(Props parameters, int bucketsCount)
        {
            bool isPreflopPocketBucketizingUsed = false;
            for (int b = bucketsCount - 1; b >= 0; --b)
            {
                string propName = "Pockets" + b.ToString();
                string value = parameters.Get(propName);
                if (!string.IsNullOrEmpty(value))
                {
                    isPreflopPocketBucketizingUsed = true;
                    break;
                }
            }

            if(!isPreflopPocketBucketizingUsed)
            {
                return;
            }

            PocketKindToAbstrCard = new int[(int)HePocketKind.__Count];

            for (int b = bucketsCount - 1; b >= 0; --b)
            {
                string propName = "Pockets" + b.ToString();
                string value = parameters.Get(propName);
                if (string.IsNullOrEmpty(value))
                {
                    if (b == 0)
                    {
                        // This can be left out.
                        continue;
                    }
                    throw new ArgumentException(string.Format("Pockets{0} is not specified", b));
                }
                string[] bucketKinds = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string kindString in bucketKinds)
                {
                    HePocketKind kind = HePocket.StringToKind(kindString);
                    PocketKindToAbstrCard[(int)kind] = b;
                }
            }
        }

        public int[] PocketKindToAbstrCard
        {
            get;
            private set;
        }
    }
}
