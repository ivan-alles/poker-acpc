/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.algorithms;
using ai.pkr.metagame;
using ai.lib.utils;

namespace ai.pkr.metastrategy.model_games
{
    /// <summary>
    /// Leduc HE bucketizer with the functionality close to open-cfr. It is very flexible and
    /// allows to create any possible bucketizer by passing a corresponding textual descriptor 
    /// (open cfr format is supported).
    /// </summary>
    public class LeducHeChanceAbstraction : IChanceAbstraction
    {
        #region Public API

        /// <summary>
        /// Equvialent of full game.
        /// </summary>
        public static readonly string FullGame = "J:Q:K:JJ:JQ:JK:QJ:QQ:QK:KJ:KQ:KK";

        /// <summary>
        /// Hand-rank bucketizing.
        /// </summary>
        public static readonly string HandRank = "J:Q:K:JQ,JK:QJ,QK:KJ,KQ:JJ,QQ,KK";

        /// <summary>
        /// Bucketizing not using private cards to bucketize shared cards.
        /// This property allows to use it with many algorithms that work with non-abstracted 
        /// games. 
        /// </summary>
        public static readonly string Public = "J:Q:K:JJ,QJ,KJ:JQ,QQ,KQ:JK,QK,KK";

        /// <summary>
        /// A lossy abstraction with exact showdown results, name is as in OCFR.
        /// </summary>
        public static readonly string JQ_K_pair_nopair = "J,Q:K:JJ,QQ:JQ,QJ,JK,QK:KK:KJ,KQ";

        /// <summary>
        /// A lossy abstraction with non-exact showdown results (pot shares will differ from exact 0, 0.5 or 1).
        /// </summary>
        public static readonly string FractionalResult = "J,Q,K:JJ,QQ,KK:JQ,QJ,QK:JK,KJ,KQ";

        /// <summary>
        /// Hand-strength bucketizer, hands with the same value of LeducHeHSMetricBucketizer.GetHandStrength() 
        /// are placed to the same bucket, the deal history is also taken into account.
        /// </summary>
        public static readonly string HandStrength = "J:Q:K:JQ,JK:JJ:QJ:QK:QQ:KJ,KQ:KK";

        /// <summary>
        /// Hand-strength bucketizer, hands with the same value of LeducHeHSMetricBucketizer.GetHandStrength() 
        /// without considering deal history.
        /// </summary>
        public static readonly string HandStrength2 = "J:Q:K:JQ,JK,QJ:QK,KJ,KQ:JJ,QQ,KK";

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="bucketsString">
        /// Format is HAND,HAND,HAND:HAND,HAND,HAND or HAND,HAND,HAND-HAND,HAND,HAND
        /// i.e. groups of hands are colon or '.' delimeted and each
        /// hand within a group is comma delimited.
        /// groups of hands must be the same type (preflop/flop) and
        /// all hands in a group will be put in the same bucket
        /// preflop hands are J, Q, K
        /// flop hands are JJ, JQ, JK, QJ, QQ, QK, KJ, KQ, KK where the first
        /// card denotes the hole card.
        /// </param>
        public LeducHeChanceAbstraction(string bucketsString)
        {
            Initialize(bucketsString);
        }

        /// <summary>
        /// Create a class with the properties, can be used for dynamic creation.
        /// <para>Properties</para>
        /// <para>BucketsString: string, required - buckets string as in OCFR.</para>
        /// </summary>
        public LeducHeChanceAbstraction(Props props)
        {
            string bucketsStrings = props.Get("BucketsString");
            Initialize(bucketsStrings);
        }


        public string Name
        {
            get;
            private set;
        }

        public int GetAbstractCard(int[] hand, int handLength)
        {
            int round = handLength-1;
            string cards = "";
            for (int i = 0; i < handLength; ++i)
            {
                cards += _cards[hand[i]];
            }
            return _handToBucket[round][cards];
        }
        #endregion


        #region Implementation

        private Dictionary<string, int>[] _handToBucket = (new Dictionary<string, int>[2]).Fill(i => new Dictionary<string, int>());
        static readonly string[] _cards = { "J", "Q", "K", "J", "Q", "K" };

        private void Initialize(string bucketsString)
        {
            string[] bucketHands = bucketsString.Split(new char[] { ':', '-' });
            int[] bucketsCount = new int[2];

            foreach (string handsString in bucketHands)
            {
                string[] hands = handsString.Split(new char[] { ',' });
                int round = hands[0].Length - 1;

                foreach (string hand in hands)
                {
                    _handToBucket[round][hand] = bucketsCount[round];
                }
                bucketsCount[round]++;
            }

            // Replace prohibited chars and commas (otherwise some tools have problems).
            string uniqueName = bucketsString.Replace(":", "-");
            uniqueName = uniqueName.Replace(",", ".");

            Name = "LeducHeCA-" + uniqueName;
        }

        #endregion


    }
}
