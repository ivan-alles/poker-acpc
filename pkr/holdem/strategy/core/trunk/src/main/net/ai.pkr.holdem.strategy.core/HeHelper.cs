/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.pkr.metagame;
using ai.lib.algorithms;
using ai.pkr.stdpoker;

namespace ai.pkr.holdem.strategy.core
{
    /// <summary>
    /// Helper functions and constants for Holdem.
    /// </summary>
    public unsafe static class HeHelper
    {
        #region Public API
        /// <summary>
        /// Converts hand size to round. For invalid hand sizes  -1.
        /// </summary>
        public static readonly int[] HandSizeToRound = new int[] {-1, -1, 0, -1, -1, 1, 2, 3};

        public static readonly int[] RoundToHandSize = new int[] { 2, 5, 6, 7};

        /// <summary>
        /// Returns true, if this hand can lose to any possible opponet's hand.
        /// If only wins or ties are possible, returns false.
        /// The hand must have the length of 7 (last round).
        /// </summary>
        public static bool CanLose(int[] hand)
        {
            int handLength = 7;
            int[] deckCopy = StdDeck.Descriptor.FullDeckIndexes.ShallowCopy();

            for (int i = 0; i < handLength; ++i)
            {
                deckCopy[hand[i]] = -1;
            }

            int boardSize = handLength - 2;
            UInt32 boardVal = 0;
            for (int i = 0; i < boardSize; ++i)
            {
                boardVal = LutEvaluator7.pLut[boardVal + hand[2 + i]];
            }

            UInt32 heroRank = LutEvaluator7.pLut[boardVal + hand[0]];
            heroRank = LutEvaluator7.pLut[heroRank + hand[1]];

            int[] restDeck = RemoveDeadCards(deckCopy, handLength);
            for (int i0 = 0; i0 < restDeck.Length - 1; ++i0)
            {
                int c0 = restDeck[i0];
                UInt32 oppRank0 = LutEvaluator7.pLut[boardVal + c0];
                for (int i1 = i0 + 1; i1 < restDeck.Length; ++i1)
                {
                    UInt32 oppRank = LutEvaluator7.pLut[oppRank0 + restDeck[i1]];
                    if (heroRank < oppRank)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Returns an array with dead card removed. Dead cards must be marked by -1.
        /// </summary>
        static int[] RemoveDeadCards(int[] deck, int deadCount)
        {
            int[] result = new int[deck.Length - deadCount];
            int cnt = 0;
            for (int i = 0; i < deck.Length; ++i)
            {
                if (deck[i] >= 0)
                {
                    result[cnt++] = deck[i];
                }
            }
            Debug.Assert(cnt == result.Length);
            return result;
        }


        #endregion
    }
}
