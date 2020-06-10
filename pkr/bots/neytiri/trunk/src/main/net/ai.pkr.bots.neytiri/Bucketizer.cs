/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.holdem;
using ai.pkr.metagame;
using ai.pkr.metastrategy;
using ai.pkr.holdem.strategy;

namespace ai.pkr.bots.neytiri
{
    public class Bucketizer
    {
        /// <summary>
        /// Number of buckets for each game round.
        /// </summary>
        public int[] BucketCount;

        public int GetBucket(CardSet pocket, CardSet board, int round)
        {
            int bucket;
            if(round == 0)
            {
                // preflop
                bucket = (int)HePockets.CardSetToPocketKind(pocket);
            }
            else
            {
                float hs = HandStrength.CalculateFast(pocket, board);
                bucket = (int)(BucketCount[round]*hs);
                if (bucket == BucketCount[round])
                    bucket--; // Special correction for a single HS value of 1.0
            }
            return bucket;
        }
    }
}
