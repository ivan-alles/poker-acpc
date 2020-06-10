/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.lib.utils;
using ai.pkr.metagame;
using ai.lib.algorithms;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Enumerates possible combinations of cards.
    /// </summary>
    public static unsafe class CardEnum
    {
        #region Public Interface

        /// <summary>
        /// Will be called from Combin to let the user do the some action on the generated combination.
        /// </summary>
        /// <param name="cards">The combination. ref is used only for performance reason. 
        /// Modification of this parameter is ignored.</param>
        public delegate void EnumerateActionDelegate(ref CardSet cards);

        /// <summary>
        /// Will be called from Combin to let the user do the some action on the generated combination.
        /// </summary>
        /// <param name="cards">The combination. ref is used only for performance reason. 
        /// Modification of this parameter is ignored.</param>
        public delegate void EnumerateActionDelegate<ParamT>(ref CardSet cards, ParamT param);

        /// <summary>
        /// Will be called from Combin to let the user do the some action on the generated combination.
        /// </summary>
        public delegate void EnumerateActionIdxDelegate<ParamT>(int [] cards, ParamT param);

        /// <summary>
        /// Combin all possible combinations of cards and calls action for each combination.
        /// </summary>
        /// <param name="count">Number of cards to enumerate (e.g. 5 for Holdem board, 2 for Holdem postflop).</param>
        /// <param name="sharedCards">Cards that must be in each combination (e.g. flop).</param>
        /// <param name="deadCards">Cards that must not be in any combination (e.g. burned cards).</param>
        /// <param name="action">Function to call.</param>
        /// <remarks>
        /// - If a card is set in both sharedMask and deadMask, "deadness" for this card is ignored. 
        ///   It will be included in each combination.
        /// 
        /// - The indexes of cards are enumerated in acsending order. That means that if the 
        ///   deck descriptor (dd) satisfies the following condition:
        ///      For any i, j: if i &lt; j then dd.CardSets[i] &lt; dd.CardSets[j] (cardsets in dd are ordered acsending)
        ///   then the resulting cardsets will be ordered in ascending order.
        ///   Our deck desciptor requires ascending order, so we can rely on ascending order of cardsets.
        /// </remarks>
        public static void Combin(DeckDescriptor deckDescr, int count, CardSet sharedCards, CardSet deadCards, EnumerateActionDelegate action)
        {
            UInt64 deadMask = deadCards.bits | sharedCards.bits;

            UInt64* pLiveCardMasks = stackalloc UInt64[deckDescr.Size];
            int liveCardsCount = 0;
            for (int c = 0; c < deckDescr.Size; ++c)
            {
                UInt64 bits = deckDescr.CardSets[c].bits;
                if ((bits & deadMask) == 0)
                    pLiveCardMasks[liveCardsCount++] = bits;
            }
            CombinInternal(sharedCards.bits, pLiveCardMasks, liveCardsCount, count, action);
        }

        /// <summary>
        /// Same as Combin, contains additional user-defined parameter that will be passed to the delegate.
        /// </summary>
        // Code is copied from non-generic Combin. Code reuse decreases performace by 10%.
        public static void Combin<ParamT>(DeckDescriptor deckDescr, int count, CardSet sharedCards, CardSet deadCards, EnumerateActionDelegate<ParamT> action, ParamT param)
        {
            UInt64 deadMask = deadCards.bits | sharedCards.bits;

            UInt64* pLiveCardMasks = stackalloc UInt64[deckDescr.Size];
            int liveCardsCount = 0;
            for (int c = 0; c < deckDescr.Size; ++c)
            {
                UInt64 bits = deckDescr.CardSets[c].bits;
                if ((bits & deadMask) == 0)
                    pLiveCardMasks[liveCardsCount++] = bits;
            }
            CombinInternal(sharedCards.bits, pLiveCardMasks, liveCardsCount, count, action, param);
        }

        /// <summary>
        /// Returns an array containing enumerated combinations.
        /// </summary>
        public static CardSet[] Combin(DeckDescriptor deckDescr, int count, CardSet sharedCards, CardSet deadCards)
        {
            CardSet unused = deadCards | sharedCards;
            int combCount = (int)EnumAlgos.CountCombin(deckDescr.Size - unused.CountCards(), count);
            CombinArrayParams p = new CombinArrayParams();
            p.arr = new CardSet[combCount];
            Combin(deckDescr, count, sharedCards, deadCards, OnCombinArray, p);
            return p.arr;
        }

        /// <summary>
        /// Combin all possible combinations of cards and calls an action for each combination.
        /// </summary>
        /// <param name="cards">Array to put the cards to.</param>
        /// <param name="startPos">Starting position in the cards array.</param>
        /// <param name="deadCards">Dead cards. If some shared cards are necessary, put it manually to both cards and deadCards.</param>
        public static void Combin<ParamT>(DeckDescriptor deckDescr, int count, int[] cards, int startPos, int[] deadCards, int deadCardsCount, EnumerateActionIdxDelegate<ParamT> action, ParamT param)
        {
            int[] deckCopy = deckDescr.FullDeckIndexes.ShallowCopy();
            for (int i = 0; i < deadCardsCount; ++i)
            {
                deckCopy[deadCards[i]] = -1;
            }
            int liveCardsLength = deckCopy.Length - deadCardsCount;
            int* liveCards = stackalloc int[liveCardsLength];
            int cnt = 0;
            for (int i1 = 0; i1 < deckCopy.Length; ++i1)
            {
                if (deckCopy[i1] >= 0)
                {
                    liveCards[cnt++] = deckCopy[i1];
                }
            }
            Debug.Assert(cnt == liveCardsLength);
            CombinInternal(liveCards, liveCardsLength, count, cards, startPos, action, param);
        }

        #endregion

        #region Implementation

        class CombinArrayParams
        {
            public CardSet[] arr;
            public int count;
        }
        static void OnCombinArray(ref CardSet cards, CombinArrayParams p)
        {
            p.arr[p.count++] = cards;
        }

        private static void CombinInternal(ulong m0, ulong* pLiveCardMasks, int liveCardsCount, int count, EnumerateActionDelegate action)
        {
            ulong m1;
            ulong m2;
            ulong m3;
            ulong m4;
            CardSet result;

            // Alogritm for liveCardsCount = 10, count = 3:
            // Live Masks:    0 1 2 3 4 5 6 7 8 9
            // 1st loop           b      ->     e
            // 2nd loop         b      ->     e
            // 3rd loop       b      ->     e

            switch (count)
            {
                case 0:
                    result.bits = m0;
                    action(ref result);
                    break;
                case 1:
                    for (int c1 = 0; c1 < liveCardsCount; ++c1)
                    {
                        result.bits = m0 | pLiveCardMasks[c1];
                        action(ref result);
                    }
                    break;
                case 2:
                    for (int c1 = 1; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 0; c2 < c1; ++c2)
                        {
                            result.bits = m1 | pLiveCardMasks[c2];
                            action(ref result);
                        }
                    }
                    break;
                case 3:
                    for (int c1 = 2; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 1; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = 0; c3 < c2; ++c3)
                            {
                                result.bits = m2 | pLiveCardMasks[c3];
                                action(ref result);
                            }
                        }
                    }
                    break;
                case 4:
                    for (int c1 = 3; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 2; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = 1; c3 < c2; ++c3)
                            {
                                m3 = m2 | pLiveCardMasks[c3];
                                for (int c4 = 0; c4 < c3; ++c4)
                                {
                                    result.bits = m3 | pLiveCardMasks[c4];
                                    action(ref result);
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    for (int c1 = 4; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 3; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = 2; c3 < c2; ++c3)
                            {
                                m3 = m2 | pLiveCardMasks[c3];
                                for (int c4 = 1; c4 < c3; ++c4)
                                {
                                    m4 = m3 | pLiveCardMasks[c4];
                                    for (int c5 = 0; c5 < c4; ++c5)
                                    {
                                        result.bits = m4 | pLiveCardMasks[c5];
                                        action(ref result);
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    for (int c1 = count - 1; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = count - 2; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = count - 3; c3 < c2; ++c3)
                            {
                                m3 = m2 | pLiveCardMasks[c3];
                                for (int c4 = count - 4; c4 < c3; ++c4)
                                {
                                    m4 = m3 | pLiveCardMasks[c4];
                                    for (int c5 = count - 5; c5 < c4; ++c5)
                                    {
                                        CombinInternal(m4 | pLiveCardMasks[c5], pLiveCardMasks, c5, count - 5, action);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private static void CombinInternal<ParamT>(ulong m0, ulong* pLiveCardMasks, int liveCardsCount, int count, EnumerateActionDelegate<ParamT> action, ParamT param)
        {
            ulong m1;
            ulong m2;
            ulong m3;
            ulong m4;
            CardSet result;

            // Alogritm for liveCardsCount = 10, count = 3:
            // Live Masks:    0 1 2 3 4 5 6 7 8 9
            // 1st loop           b      ->     e
            // 2nd loop         b      ->     e
            // 3rd loop       b      ->     e

            switch (count)
            {
                case 0:
                    result.bits = m0;
                    action(ref result, param);
                    break;
                case 1:
                    for (int c1 = 0; c1 < liveCardsCount; ++c1)
                    {
                        result.bits = m0 | pLiveCardMasks[c1];
                        action(ref result, param);
                    }
                    break;
                case 2:
                    for (int c1 = 1; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 0; c2 < c1; ++c2)
                        {
                            result.bits = m1 | pLiveCardMasks[c2];
                            action(ref result, param);
                        }
                    }
                    break;
                case 3:
                    for (int c1 = 2; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 1; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = 0; c3 < c2; ++c3)
                            {
                                result.bits = m2 | pLiveCardMasks[c3];
                                action(ref result, param);
                            }
                        }
                    }
                    break;
                case 4:
                    for (int c1 = 3; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 2; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = 1; c3 < c2; ++c3)
                            {
                                m3 = m2 | pLiveCardMasks[c3];
                                for (int c4 = 0; c4 < c3; ++c4)
                                {
                                    result.bits = m3 | pLiveCardMasks[c4];
                                    action(ref result, param);
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    for (int c1 = 4; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = 3; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = 2; c3 < c2; ++c3)
                            {
                                m3 = m2 | pLiveCardMasks[c3];
                                for (int c4 = 1; c4 < c3; ++c4)
                                {
                                    m4 = m3 | pLiveCardMasks[c4];
                                    for (int c5 = 0; c5 < c4; ++c5)
                                    {
                                        result.bits = m4 | pLiveCardMasks[c5];
                                        action(ref result, param);
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    for (int c1 = count - 1; c1 < liveCardsCount; ++c1)
                    {
                        m1 = m0 | pLiveCardMasks[c1];
                        for (int c2 = count - 2; c2 < c1; ++c2)
                        {
                            m2 = m1 | pLiveCardMasks[c2];
                            for (int c3 = count - 3; c3 < c2; ++c3)
                            {
                                m3 = m2 | pLiveCardMasks[c3];
                                for (int c4 = count - 4; c4 < c3; ++c4)
                                {
                                    m4 = m3 | pLiveCardMasks[c4];
                                    for (int c5 = count - 5; c5 < c4; ++c5)
                                    {
                                        CombinInternal(m4 | pLiveCardMasks[c5], pLiveCardMasks, c5, count - 5, action, param);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Enumerate indexes.
        /// </summary>
        /// Developer notes: I tried to use a pointer for cards[] too, but it does not changes the speed.
        static void CombinInternal<ParamT>(int * liveCards, int liveCardsCount, int count, int[] cards, int startPos, EnumerateActionIdxDelegate<ParamT> action, ParamT param)
        {
            switch (count)
            {
                case 0:
                    action(cards, param);
                    break;
                case 1:
                    for (int c1 = 0; c1 < liveCardsCount; ++c1)
                    {
                        cards[startPos] = liveCards[c1];
                        action(cards, param);
                    }
                    break;
                case 2:
                    for (int c1 = 1; c1 < liveCardsCount; ++c1)
                    {
                        cards[startPos] = liveCards[c1];
                        for (int c2 = 0; c2 < c1; ++c2)
                        {
                            cards[startPos+1] = liveCards[c2];
                            action(cards, param);
                        }
                    }
                    break;
                case 3:
                    for (int c1 = 2; c1 < liveCardsCount; ++c1)
                    {
                        cards[startPos] = liveCards[c1];
                        for (int c2 = 1; c2 < c1; ++c2)
                        {
                            cards[startPos + 1] = liveCards[c2];
                            for (int c3 = 0; c3 < c2; ++c3)
                            {
                                cards[startPos + 2] = liveCards[c3];
                                action(cards, param);
                            }
                        }
                    }
                    break;
                case 4:
                    for (int c1 = 3; c1 < liveCardsCount; ++c1)
                    {
                        cards[startPos] = liveCards[c1];
                        for (int c2 = 2; c2 < c1; ++c2)
                        {
                            cards[startPos + 1] = liveCards[c2];
                            for (int c3 = 1; c3 < c2; ++c3)
                            {
                                cards[startPos + 2] = liveCards[c3];
                                for (int c4 = 0; c4 < c3; ++c4)
                                {
                                    cards[startPos + 3] = liveCards[c4];
                                    action(cards, param);
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    for (int c1 = 4; c1 < liveCardsCount; ++c1)
                    {
                        cards[startPos] = liveCards[c1];
                        for (int c2 = 3; c2 < c1; ++c2)
                        {
                            cards[startPos + 1] = liveCards[c2];
                            for (int c3 = 2; c3 < c2; ++c3)
                            {
                                cards[startPos + 2] = liveCards[c3];
                                for (int c4 = 1; c4 < c3; ++c4)
                                {
                                    cards[startPos + 3] = liveCards[c4];
                                    for (int c5 = 0; c5 < c4; ++c5)
                                    {
                                        cards[startPos + 4] = liveCards[c5];
                                        action(cards, param);
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    for (int c1 = count - 1; c1 < liveCardsCount; ++c1)
                    {
                        cards[startPos] = liveCards[c1];
                        for (int c2 = count - 2; c2 < c1; ++c2)
                        {
                            cards[startPos + 1] = liveCards[c2];
                            for (int c3 = count - 3; c3 < c2; ++c3)
                            {
                                cards[startPos + 2] = liveCards[c3];
                                for (int c4 = count - 4; c4 < c3; ++c4)
                                {
                                    cards[startPos + 3] = liveCards[c4];
                                    for (int c5 = count - 5; c5 < c4; ++c5)
                                    {
                                        cards[startPos + 4] = liveCards[c5];
                                        CombinInternal(liveCards, c5, count - 5, cards, startPos + 5, action, param);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}