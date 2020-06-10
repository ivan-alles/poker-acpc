/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.algorithms;
using ai.lib.algorithms.numbers;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Converts a hand to a hand with the same card ranks, fully ignoring suits.
    /// All suits are transformed to the minimal possible. 
    /// For example: AcKs -> AcKc, 2s2h2c -> 2c2d2h. 
    /// </summary>
    /// <remarks>The deck must follow the layout conventions of DeckDescriptor.</remarks>
    public static class NormRank
    {
        private static readonly UInt64[] _rankMasks = new UInt64[16];

        static NormRank()
        {
            for(int r = 0; r < 16; ++r)
            {
                _rankMasks[r] = 0x0001000100010001ul << r;
            }
        }

        /// <summary>
        /// Converts the hand. Can be called subsequently e.g. on pocket, flop, turn and river.
        /// </summary>
        public static CardSet Convert(CardSet hand)
        {
            uint half = (uint) hand.bits;
            uint s0 = (half & 0xFFFF);
            uint s1 = (half >> 16);
            half = (uint) (hand.bits >> 32);
            uint s2 = (half & 0xFFFF);
            uint s3 = (half >> 16);

            // Result suits
            // r0 - everything that is present at least in one of s0..s3.
            // r1 - ... at least in 2 of s0..s3.
            // r2 - ... at least in 3 of s0..s3.
            // r3 - present in each of s0..s3.

            uint r0 = s0 | s1 | s2 | s3;
            uint r1 = (s0 & s1) | (s0 & s2) | (s0 & s3) | (s1 & s2) | (s1 & s3) | (s2 & s3);
            uint r2 = (s0 & s1 & s2) | (s0 & s1 & s3) | (s0 & s2 & s3) | (s1 & s2 & s3);
            uint r3 = s0 & s1 & s2 & s3;

            CardSet r = new CardSet();
            r.bits = r0 | (r1 << 16);
            r.bits |= (UInt64)(r2 | (r3 << 16)) << 32;
            return r;
        }

        /// <summary>
        /// Counts number of rank-equivalent hands that are comprised from the same cards
        /// ranks as the given hand, fully ignoring suits. 
        /// For example, for AcKc or AsKh returns 16, for 2s2d returns 6. 
        /// There must be no dead cards.
        /// </summary>
        /// <remarks>Works even if a deck contains different number of suits (&lt;=4) for different ranks.
        /// </remarks>
        public static int CountEquiv(CardSet hand, DeckDescriptor deck)
        {
            int count = 1;
            for (int r = 0; r < 16; ++r)
            {
                UInt64 handRanks = hand.bits & _rankMasks[r];
                UInt64 allRanks = deck.FullDeck.bits & _rankMasks[r];
                int handRanksCount = CountBits.Count(handRanks);
                int allRanksCount = CountBits.Count(allRanks);
                int rankCount = (int)EnumAlgos.CountCombin(allRanksCount, handRanksCount);
                count *= rankCount;
            }
            return count;
        }
    }
}