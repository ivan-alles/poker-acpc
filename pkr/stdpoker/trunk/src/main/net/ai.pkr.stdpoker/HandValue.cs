/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.algorithms.lut;
using System.Diagnostics;

namespace ai.pkr.stdpoker
{
    /// <summary>
    /// Format and operations with hand value (e.g. straight, flush, etc.)
    /// </summary>
    public static unsafe class HandValue
    {
        public enum Kind
        {
            HighCard = 0,
            Pair = 1,
            TwoPair = 2,
            Trips = 3,
            Straight = 4,
            Flush = 5,
            FullHouse = 6,
            FourOfAKind = 7,
            StraightFlush = 8,
            _Count
        };

        public static Kind GetKind(UInt32 handValue)
        {
            return (Kind)(handValue >> SHIFT_HANDTYPE);
        }

        public static string ValueToString(CardSet hand, UInt32 value)
        {
            StringBuilder result = new StringBuilder();
            HandValue.Kind handType = (HandValue.Kind)(value >> SHIFT_HANDTYPE);
            switch (handType)
            {
                case HandValue.Kind.HighCard:
                    result.AppendFormat("High Card: {0}", CombinationToString(hand, value, 5));
                    break;
                case HandValue.Kind.Pair:
                    result.AppendFormat("Pair: {0}", CombinationToString(hand, value, 4));
                    break;
                case HandValue.Kind.TwoPair:
                    result.AppendFormat("2 Pair: {0}", CombinationToString(hand, value, 3));
                    break;
                case HandValue.Kind.Trips:
                    result.AppendFormat("3 of a Kind: {0}", CombinationToString(hand, value, 3));
                    break;
                case HandValue.Kind.Straight:
                    result.AppendFormat("Straight: {0}", StraightToString(hand, value));
                    break;
                case HandValue.Kind.Flush:
                    result.AppendFormat("Flush: {0}", FlushToString(hand, value, false));
                    break;
                case HandValue.Kind.FullHouse:
                    result.AppendFormat("Full House: {0}", CombinationToString(hand, value, 2));
                    break;
                case HandValue.Kind.FourOfAKind:
                    result.AppendFormat("4 of a Kind: {0}", CombinationToString(hand, value, 2));
                    break;
                case HandValue.Kind.StraightFlush:
                    result.AppendFormat("Straight Flush: {0}", FlushToString(hand, value, true));
                    break;
            }
            return result.ToString();
        }

        #region Implementation

        internal const int SHIFT_HANDTYPE = 24;
        internal const int SHIFT_CARD_1 = 16;
        internal const int CARD_WIDTH = 4;

        private static string CombinationToString(CardSet hand, UInt32 value, int count)
        {
            StringBuilder result = new StringBuilder(16);
            for (int i = 0; i < count; ++i)
            {
                int rank = (int)(0xF & (value >> (int)(SHIFT_CARD_1 - CARD_WIDTH * i)));
                for (int s = 0; s < 4; ++s)
                {
                    CardSet cardset = new CardSet();
                    cardset.bits = hand.bits & (1UL << (int)(16 * s + rank));
                    if (cardset.bits != 0)
                    {
                        result.Append(StdDeck.Descriptor.GetCardNames(cardset));
                        result.Append(' ');
                    }
                }
            }
            result.Remove(result.Length - 1, 1);
            return result.ToString();
        }

        private static string StraightToString(CardSet hand, UInt32 value)
        {
            StringBuilder result = new StringBuilder(16);
            int rank = (int)(0xF & (value >> SHIFT_CARD_1));
            for (int i = 0; i < 5; ++i, rank--)
            {
                if (rank < 0) rank = 12; // For 5-high straight.
                for (int s = 0; s < 4; ++s)
                {
                    CardSet cardset = new CardSet();
                    cardset.bits = hand.bits & (1UL << (int)(16 * s + rank));
                    if (cardset.bits != 0)
                    {
                        result.Append(StdDeck.Descriptor.GetCardNames(cardset));
                        result.Append(' ');
                        break; // Use only 1 card of each suite.
                    }
                }
            }
            result.Remove(result.Length - 1, 1);
            return result.ToString();
        }

        private static string FlushToString(CardSet hand, UInt32 value, bool isStraightFlush)
        {
            UInt32 sc = (UInt32)hand.bits;
            UInt32 sd = sc >> 16;
            sc &= 0x1FFF;
            UInt32 sh = (UInt32)(hand.bits >> 32);
            UInt32 ss = sh >> 16;
            sh &= 0x1FFF;

            CardSet suitedHand = new CardSet();

            if (LutBitCount.T[sc] >= 5) suitedHand.bits = sc;
            else if (LutBitCount.T[sd] >= 5) suitedHand.bits = (UInt64)sd << 16;
            else if (LutBitCount.T[sh] >= 5) suitedHand.bits = (UInt64)sh << 32;
            else if (LutBitCount.T[ss] >= 5) suitedHand.bits = (UInt64)ss << 48;
            else Debug.Assert(false);

            return isStraightFlush ?
                StraightToString(suitedHand, value) :
                CombinationToString(suitedHand, value, 5);
        }

        #endregion
    }
}
