/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.pkr.stdpoker.generated;
using ai.lib.utils;
using ai.lib.algorithms.lut;

namespace ai.pkr.stdpoker
{
    /// <summary>
    /// Evaluates poker hand strenghts.
    /// Based on poker-eval algorithm.
    /// </summary>
    public static unsafe class CardSetEvaluator
    {


        #region Public interface

        /// <summary>
        /// Evaluates a hand.
        /// </summary>
        /// <param name="hand">The hand. ref is used to speed-up parameter passing, 
        /// actually the value is not modified</param>
        /// <returns>Hand rank, a number > 0, the greater the number the stronger the card.</returns>
        public static uint Evaluate(ref CardSet hand)
        {
            UInt32 ranks, four_mask, three_mask, two_mask, n_ranks, second;

            // Calculate suite mask, minimizing 64-bit shifts.
            UInt32 sc = (UInt32)hand.bits;
            UInt32 sd = sc >> 16;
            sc &= 0x1FFF;
            UInt32 sh = (UInt32)(hand.bits >> 32);
            UInt32 ss = sh >> 16;
            sh &= 0x1FFF;

            ranks = sc | sd | sh | ss;
            n_ranks = pLutBitCount[ranks];

            // Check for straight, flush, or straight flush, and return, because in this case
            //   no better hand possible for max 7-hands.
            if (n_ranks >= 5)
            {
                if (pLutBitCount[ss] >= 5)
                {
                    if (pLutStraight[ss] != 0)
                        return HT_STRAIGHT_FLUSH + ((UInt32)pLutStraight[ss] << SHIFT_CARD_1);
                    else
                        return HT_FLUSH + pLutTopFiveCards[ss];
                }
                else if (pLutBitCount[sc] >= 5)
                {
                    if (pLutStraight[sc] != 0)
                        return HT_STRAIGHT_FLUSH + ((UInt32)pLutStraight[sc] << SHIFT_CARD_1);
                    else
                        return HT_FLUSH + pLutTopFiveCards[sc];
                }
                else if (pLutBitCount[sd] >= 5)
                {
                    if (pLutStraight[sd] != 0)
                        return HT_STRAIGHT_FLUSH + ((UInt32)pLutStraight[sd] << SHIFT_CARD_1);
                    else
                        return HT_FLUSH + pLutTopFiveCards[sd];
                }
                else if (pLutBitCount[sh] >= 5)
                {
                    if (pLutStraight[sh] != 0)
                        return HT_STRAIGHT_FLUSH + ((UInt32)pLutStraight[sh] << SHIFT_CARD_1);
                    else
                        return HT_FLUSH + pLutTopFiveCards[sh];
                }
                else
                {
                    UInt32 st = pLutStraight[ranks];
                    if (st != 0)
                        return HT_STRAIGHT + (st << SHIFT_CARD_1);
                }
            };

            int cardCount = pLutBitCount[sc] + pLutBitCount[sd] + pLutBitCount[sh] + pLutBitCount[ss];  

            UInt32 retval;
            UInt32 n_dups = (UInt32)cardCount - n_ranks;

            // No flush or straight or straight flush possible.
            switch (n_dups)
            {
                case 0:
                    /* It's a no-pair hand */
                    return HT_HIGH_CARD + pLutTopFiveCards[ranks];

                case 1:
                    {
                        /* It's a one-pair hand */
                        UInt32 t, kickers;

                        two_mask = ranks ^ (sc ^ sd ^ sh ^ ss);

                        retval = HT_PAIR + (pLutTopCard[two_mask] << SHIFT_CARD_1);
                        t = ranks ^ two_mask;      /* Only one bit set in two_mask */
                        /* Get the top five cards in what is left, drop all but the top three 
                         * cards, and shift them by one to get the three desired kickers */
                        kickers = (pLutTopFiveCards[t] >> CARD_WIDTH) & ~MASK_CARD_5;
                        retval += kickers;
                        return retval;
                    }

                case 2:
                    /* Either two pair or trips */
                    two_mask = ranks ^ (sc ^ sd ^ sh ^ ss);
                    if (two_mask != 0)
                    {
                        UInt32 t;
                        t = ranks ^ two_mask; /* Exactly two bits set in two_mask */
                        retval = HT_TWO_PAIR
                          + (pLutTopFiveCards[two_mask] & (MASK_CARD_1 | MASK_CARD_2))
                          + (pLutTopCard[t] << SHIFT_CARD_3);
                        return retval;
                    }
                    else
                    {
                        UInt32 t;
                        three_mask = ((sc & sd) | (sh & ss)) & ((sc & sh) | (sd & ss));

                        retval = HT_TRIPS + (pLutTopCard[three_mask] << SHIFT_CARD_1);

                        t = ranks ^ three_mask; /* Only one bit set in three_mask */
                        second = pLutTopCard[t];
                        retval += (second << SHIFT_CARD_2);
                        t ^= (1U << (int)second);
                        retval += (pLutTopCard[t] << SHIFT_CARD_3);
                        return retval;
                    }

                default:
                    /* Possible quads, fullhouse, straight or flush, or two pair */
                    four_mask = sh & sd & sc & ss;
                    if (four_mask != 0)
                    {
                        UInt32 tc;
                        tc = pLutTopCard[four_mask];
                        retval = HT_FOUR_OF_A_KIND
                          + (tc << SHIFT_CARD_1)
                          + ((pLutTopCard[ranks ^ (1U << (int)tc)]) << SHIFT_CARD_2);
                        return retval;
                    }

                    /* Technically, three_mask as defined below is really the set of
                       bits which are set in three or four of the suits, but since
                       we've already eliminated quads, this is OK */
                    /* Similarly, two_mask is really two_or_four_mask, but since we've
                       already eliminated quads, we can use this shortcut */

                    two_mask = ranks ^ (sc ^ sd ^ sh ^ ss);
                    if (pLutBitCount[two_mask] != n_dups)
                    {
                        /* Must be some trips then, which really means there is a 
                           full house since n_dups >= 3 */
                        UInt32 tc, t;

                        three_mask = ((sc & sd) | (sh & ss)) & ((sc & sh) | (sd & ss));
                        retval = HT_FULL_HOUSE;
                        tc = pLutTopCard[three_mask];
                        retval += (tc << SHIFT_CARD_1);
                        t = (two_mask | three_mask) ^ (1U << (int)tc);
                        retval += (pLutTopCard[t] << SHIFT_CARD_2);
                        return retval;
                    }
                    /* Must be two pair */
                    UInt32 top;
                    retval = HT_TWO_PAIR;
                    top = pLutTopCard[two_mask];
                    retval += (top << SHIFT_CARD_1);
                    second = pLutTopCard[two_mask ^ (1U << (int)top)];
                    retval += (second << SHIFT_CARD_2);
                    retval += (pLutTopCard[ranks ^ (1U << (int)top) ^ (1U << (int)second)]) << SHIFT_CARD_3;
                    return retval;
            }

            // Debug.Assert(false, "Logic error");
        }


        #endregion

        #region Private Constants

        private const int SHIFT_HANDTYPE = HandValue.SHIFT_HANDTYPE;
        private const int SHIFT_CARD_1 = HandValue.SHIFT_CARD_1;
        private const UInt32 MASK_CARD_1 = 0x000F0000;
        private const int SHIFT_CARD_2 = 12;
        private const UInt32 MASK_CARD_2 = 0x0000F000;
        private const int SHIFT_CARD_3 = 8;
        private const UInt32 MASK_CARD_5 = 0x0000000F;
        private const int CARD_WIDTH = HandValue.CARD_WIDTH;

        private const UInt32 HT_STRAIGHT_FLUSH = ((UInt32)HandValue.Kind.StraightFlush) << SHIFT_HANDTYPE;
        private const UInt32 HT_STRAIGHT = ((UInt32)HandValue.Kind.Straight) << SHIFT_HANDTYPE;
        private const UInt32 HT_FLUSH = ((UInt32)HandValue.Kind.Flush) << SHIFT_HANDTYPE;
        private const UInt32 HT_FULL_HOUSE = ((UInt32)HandValue.Kind.FullHouse) << SHIFT_HANDTYPE;
        private const UInt32 HT_FOUR_OF_A_KIND = ((UInt32)HandValue.Kind.FourOfAKind) << SHIFT_HANDTYPE;
        private const UInt32 HT_TRIPS = ((UInt32)HandValue.Kind.Trips) << SHIFT_HANDTYPE;
        private const UInt32 HT_TWO_PAIR = ((UInt32)HandValue.Kind.TwoPair) << SHIFT_HANDTYPE;
        private const UInt32 HT_PAIR = ((UInt32)HandValue.Kind.Pair) << SHIFT_HANDTYPE;
        private const UInt32 HT_HIGH_CARD = ((UInt32)HandValue.Kind.HighCard) << SHIFT_HANDTYPE;

        #endregion

        #region Private Members
        // Use local static variables to access LUTs, it's faster.
        static readonly byte* pLutBitCount = LutBitCount.T;
        static readonly byte* pLutStraight = LutStraight.T;
        static readonly UInt32* pLutTopFiveCards = LutTopFiveCards.T;
        static readonly UInt32* pLutTopCard = LutTopCard.T;
        static readonly UInt64* pLutCardMask;
        static readonly SmartPtr _LutCardMaskPtr;

        static CardSetEvaluator()
        {
            _LutCardMaskPtr = UnmanagedMemory.AllocHGlobalExSmartPtr(StdDeck.Descriptor.Size * sizeof(UInt64));
            pLutCardMask = (UInt64*) _LutCardMaskPtr;
            for (int i = 0; i < StdDeck.Descriptor.Size; ++i)
            {
                pLutCardMask[i] = StdDeck.Descriptor.CardSets[i].bits;
            }
        }

        #endregion

    }
}
